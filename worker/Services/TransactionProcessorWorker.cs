using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Worker.Infrastructure;
using Transactions.Worker.Models;
using Transactions.Worker.Options;

namespace Transactions.Worker.Services;

public sealed class TransactionProcessorWorker : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly WorkerOptions _workerOptions;
    private readonly BalanceCalculatorFactory _balanceCalculatorFactory;
    private readonly ILogger<TransactionProcessorWorker> _logger;

    private readonly object _updateLock = new();
    private int _transactionsCounter;
    private DateTimeOffset? _batchStart;
    private IBalanceCalculator? _balanceCalculator;

    public TransactionProcessorWorker(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<WorkerOptions> workerOptions,
        BalanceCalculatorFactory balanceCalculatorFactory,
        ILogger<TransactionProcessorWorker> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _workerOptions = workerOptions.Value;
        _balanceCalculatorFactory = balanceCalculatorFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            Acks = Acks.All,
            MessageTimeoutMs = _kafkaOptions.ProduceTimeoutMs
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        consumer.Subscribe(_kafkaOptions.ConsumerTopic);
        _logger.LogInformation("Worker consuming from topic {Topic}", _kafkaOptions.ConsumerTopic);

        var nextBatchCheck = DateTimeOffset.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var record = consumer.Consume(TimeSpan.FromMilliseconds(200));
                if (record?.Message is not null)
                {
                    await ConsumeAndProcessAsync(record, producer, stoppingToken);
                }

                if (DateTimeOffset.UtcNow >= nextBatchCheck)
                {
                    await CompleteBatchAsync(stoppingToken);
                    nextBatchCheck = DateTimeOffset.UtcNow.AddMilliseconds(_workerOptions.BatchIntervalMs);
                }
            }
            catch (ConsumeException ex)
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
        consumer.Close();
    }

    private async Task ConsumeAndProcessAsync(
        ConsumeResult<string, string> consumedRecord,
        IProducer<string, string> producer,
        CancellationToken cancellationToken)
    {
        var key = consumedRecord.Message.Key;
        var transaction = JsonSerializer.Deserialize<Transaction>(consumedRecord.Message.Value, WorkerJson.Options);
        if (transaction is null)
        {
            _logger.LogWarning("Consumed null/invalid payload from Kafka: {Payload}", consumedRecord.Message.Value);
            return;
        }

        _logger.LogInformation("Consumed payload from Kafka: {@Transaction}", transaction);

        await PrepareAsync(transaction, cancellationToken);

        string status;
        lock (_updateLock)
        {
            _transactionsCounter++;
            var transactionId = transaction.TransactionId ?? 0;
            var amount = transaction.Amount ?? 0m;
            status = _balanceCalculator?.Execute(transactionId, amount) ?? "ERROR";
            transaction.Status = status;
        }

        var response = JsonSerializer.Serialize(transaction, WorkerJson.Options);
        var outMsg = new Message<string, string>
        {
            Key = key,
            Value = response,
            Headers = new Headers()
        };

        var correlationIdHeader = consumedRecord.Message.Headers?.FirstOrDefault(h => h.Key == "correlationId");
        if (correlationIdHeader is not null)
        {
            outMsg.Headers.Add("correlationId", correlationIdHeader.GetValueBytes());
        }

        _logger.LogInformation("Producing payload to Kafka with key {Key}: {@Transaction}", key, transaction);

        try
        {
            await producer.ProduceAsync(_kafkaOptions.ProducerTopic, outMsg, cancellationToken);
        }
        catch (ProduceException<string, string> ex)
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
        if (transaction.Account is null)
        {
            return;
        }

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

        var accountId = transaction.Account.ToAccountId();
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
