using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Bff.App.Config;
using Transactions.Bff.Domain.Models;
using Transactions.Bff.Infrastructure.Kafka;
using Transactions.Bff.Infrastructure.Options;

namespace Transactions.Bff.Infrastructure.Messaging;

public sealed class TransactionConsumer(
    TransactionConsumerBuilder builder,
    IOptions<KafkaConfig> options,
    CorrelationStore store,
    ILogger<TransactionConsumer> logger) : BackgroundService
{
    private readonly KafkaConfig _options = options.Value;
    private readonly CorrelationStore _store = store;
    private readonly ILogger<TransactionConsumer> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = await builder.Build();
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
                var transaction = JsonSerializer.Deserialize<Transaction>(result.Message.Value, JsonOptions.Serialization);
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
