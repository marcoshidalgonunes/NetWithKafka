using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Bff.Infrastructure;
using Transactions.Bff.Models;
using Transactions.Bff.Options;

namespace Transactions.Bff.Services;

public sealed class KafkaReplyConsumerService : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly KafkaCorrelationStore _store;
    private readonly ILogger<KafkaReplyConsumerService> _logger;

    public KafkaReplyConsumerService(
        IOptions<KafkaOptions> options,
        KafkaCorrelationStore store,
        ILogger<KafkaReplyConsumerService> logger)
    {
        _options = options.Value;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_options.ConsumerTopic);

        _logger.LogInformation("Kafka reply consumer started on topic {Topic}", _options.ConsumerTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message is null)
                {
                    continue;
                }

                _logger.LogInformation("Consumed Transaction from Kafka: {Payload}", result.Message.Value);

                var correlationHeader = result.Message.Headers?.FirstOrDefault(h => h.Key == "correlationId");
                if (correlationHeader is null)
                {
                    _logger.LogWarning("Reply message has no correlationId header");
                    continue;
                }

                var correlationId = Encoding.UTF8.GetString(correlationHeader.GetValueBytes());
                var transaction = JsonSerializer.Deserialize<Transaction>(result.Message.Value, BffJson.Options);
                if (transaction is null)
                {
                    _logger.LogWarning("Failed to deserialize transaction reply for correlationId {CorrelationId}", correlationId);
                    continue;
                }

                _store.TryComplete(correlationId, transaction);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
                await Task.Delay(500, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected consumer error");
                await Task.Delay(500, stoppingToken);
            }
        }

        consumer.Close();
    }
}
