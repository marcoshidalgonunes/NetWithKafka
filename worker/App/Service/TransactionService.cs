using System.Text.Json;
using Microsoft.Extensions.Options;
using Transactions.Worker.App.Config;
using Transactions.Worker.Domain.Models;
using Transactions.Worker.Infrastructure.Options;
using Transactions.Worker.App.Engine;
using Transactions.Worker.Domain.Contracts;
using Transactions.Worker.Infrastructure.Kafka;

namespace Transactions.Worker.App.Service;

public sealed class TransactionService(
    TransactionConsumer transactionConsumer,
    TransactionProducer transactionProducer,
    IOptions<KafkaConfig> kafkaOptions,
    IOptions<WorkerConfig> workerOptions,
    BalanceCalculatorEngineFactory balanceCalculatorFactory,
    ILogger<TransactionService> logger) : BackgroundService
{
    private readonly TransactionConsumer _transactionConsumer = transactionConsumer;
    private readonly TransactionProducer _transactionProducer = transactionProducer;
    private readonly KafkaConfig _kafkaOptions = kafkaOptions.Value;
    private readonly WorkerConfig _workerOptions = workerOptions.Value;
    private readonly BalanceCalculatorEngineFactory _balanceCalculatorFactory = balanceCalculatorFactory;
    private readonly ILogger<TransactionService> _logger = logger;

    private readonly object _updateLock = new();
    private int _transactionsCounter;
    private DateTimeOffset? _batchStart;
    private IBalanceCalculator? _balanceCalculator;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _transactionConsumer.Subscribe(_kafkaOptions.ConsumerTopic);
        _logger.LogInformation("Worker consuming from topic {Topic}", _kafkaOptions.ConsumerTopic);

        var nextBatchCheck = DateTimeOffset.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var record = _transactionConsumer.Consume(TimeSpan.FromMilliseconds(200));
                if (record is not null)
                {
                    await ConsumeAndProcessAsync(record, stoppingToken);
                }

                if (DateTimeOffset.UtcNow >= nextBatchCheck)
                {
                    await CompleteBatchAsync(stoppingToken);
                    nextBatchCheck = DateTimeOffset.UtcNow.AddMilliseconds(_workerOptions.BatchIntervalMs);
                }
            }
            catch (KafkaConsumerException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected worker error");
            }
        }

        await CompleteBatchAsync(stoppingToken);
    }

    private async Task ConsumeAndProcessAsync(
        ConsumedMessage consumedRecord,
        CancellationToken cancellationToken)
    {
        var correlationId = consumedRecord.Key ?? string.Empty;
        var transaction = JsonSerializer.Deserialize<Transaction>(consumedRecord.Value, JsonOptions.Serialization);
        if (transaction is null)
        {
            _logger.LogWarning("Consumed null/invalid payload from Kafka: {Payload}", consumedRecord.Value);
            return;
        }

        _logger.LogInformation("Consumed payload from Kafka: {@Transaction}", transaction);

        await PrepareAsync(transaction, cancellationToken);

        string status;
        lock (_updateLock)
        {
            _transactionsCounter++;
            var transactionId = transaction.TransactionId;
            var amount = transaction.Entry.Amount;
            status = _balanceCalculator?.Compute(transactionId, amount) ?? "ERROR";
            transaction.Status = status;
        }

        var response = JsonSerializer.Serialize(transaction, JsonOptions.Serialization);

        _logger.LogInformation("Producing payload to Kafka with key {Key}: {@Transaction}", correlationId, transaction);
        try
        {
            await _transactionProducer.ProduceAsync(correlationId, response, cancellationToken);
        }
        catch (KafkaProducerException ex)
        {
            _logger.LogError(ex, "Kafka send failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Kafka send");
        }
    }

    private async Task PrepareAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var shouldPrepare = false;
        lock (_updateLock)
        {
            if (_transactionsCounter == 0)
            {
                shouldPrepare = true;
            }
        }

        if (!shouldPrepare)
        {
            return;
        }

        var accountId = transaction.Entry.Account.ToAccountId();
        var calculator = await _balanceCalculatorFactory.CreateAsync(accountId, cancellationToken);

        lock (_updateLock)
        {
            if (_transactionsCounter == 0)
            {
                _balanceCalculator = calculator;
                _batchStart = DateTimeOffset.UtcNow;
            }
        }
    }

    private async Task CompleteBatchAsync(CancellationToken cancellationToken)
    {
        IBalanceCalculator? calculatorToUpdate = null;
        int count = 0;
        long duration = 0;

        lock (_updateLock)
        {
            if (_batchStart.HasValue)
            {
                duration = (long)(DateTimeOffset.UtcNow - _batchStart.Value).TotalMilliseconds;
                if (_transactionsCounter >= _workerOptions.BatchFlushThreshold || duration >= _workerOptions.BatchFlushMinAgeMs)
                {
                    calculatorToUpdate = _balanceCalculator;
                    count = _transactionsCounter;
                    _transactionsCounter = 0;
                    _batchStart = null;
                    _balanceCalculator = null;
                }
            }
        }

        if (calculatorToUpdate is null)
        {
            return;
        }

        _logger.LogInformation("Completing batch of {Count} transactions after {Duration} ms", count, duration);
        await calculatorToUpdate.UpdateAsync(cancellationToken);
    }
}
