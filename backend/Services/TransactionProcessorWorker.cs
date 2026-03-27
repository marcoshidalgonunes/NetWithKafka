using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Backend.Infrastructure;
using Transactions.Backend.Models;
using Transactions.Backend.Options;

namespace Transactions.Backend.Services;

public sealed class TransactionProcessorWorker : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly TransactionProcessorLogic _logic;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<TransactionProcessorWorker> _logger;

    public TransactionProcessorWorker(
        IOptions<KafkaOptions> kafkaOptions,
        TransactionProcessorLogic logic,
        IProducer<string, string> producer,
        ILogger<TransactionProcessorWorker> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logic = logic;
        _producer = producer;
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

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(_kafkaOptions.ConsumerTopic);

        _logger.LogInformation("Backend consuming from topic {Topic}", _kafkaOptions.ConsumerTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumedRecord = consumer.Consume(stoppingToken);
                if (consumedRecord?.Message is null)
                {
                    continue;
                }

                var key = consumedRecord.Message.Key;
                var transaction = JsonSerializer.Deserialize<Transaction>(consumedRecord.Message.Value, BackendJson.Options);
                if (transaction is null)
                {
                    _logger.LogWarning("Consumed null/invalid payload from Kafka: {Payload}", consumedRecord.Message.Value);
                    continue;
                }

                _logger.LogInformation("Consumed payload from Kafka: {@Transaction}", transaction);

                var processedResult = await _logic.ProcessAsync(transaction, stoppingToken);

                var produceMessage = new Message<string, string>
                {
                    Key = key,
                    Value = JsonSerializer.Serialize(processedResult, BackendJson.Options),
                    Headers = new Headers()
                };

                var correlationHeader = consumedRecord.Message.Headers?.FirstOrDefault(h => h.Key == "correlationId");
                if (correlationHeader is not null)
                {
                    produceMessage.Headers.Add("correlationId", correlationHeader.GetValueBytes());
                }

                _logger.LogInformation("Producing payload to Kafka with key {Key}: {@Transaction}", key, processedResult);
                await _producer.ProduceAsync(_kafkaOptions.ProducerTopic, produceMessage, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume failed");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Kafka send failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during backend processing loop");
            }
        }

        _producer.Flush(TimeSpan.FromSeconds(5));
        consumer.Close();
    }
}
