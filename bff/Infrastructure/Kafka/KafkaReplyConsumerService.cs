using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using Transactions.Bff.App.Config;
using Transactions.Bff.Domain.Models;
using Transactions.Bff.Infrastructure.Options;

namespace Transactions.Bff.Infrastructure.Kafka;

public sealed class KafkaReplyConsumerService(
    IOptions<KafkaConfig> options,
    CorrelationStore store,
    ILogger<KafkaReplyConsumerService> logger) : BackgroundService
{
    private readonly KafkaConfig _options = options.Value;
    private readonly CorrelationStore _store = store;
    private readonly ILogger<KafkaReplyConsumerService> _logger = logger;

    private async Task EnsureTopicsExistAsync(CancellationToken stoppingToken)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _options.BootstrapServers
        }).Build();

        var topics = new[]
        {
            new TopicSpecification { Name = _options.ConsumerTopic, NumPartitions = 3, ReplicationFactor = 1 },
            new TopicSpecification { Name = _options.ProducerTopic, NumPartitions = 3, ReplicationFactor = 1 }
        };

        try
        {
            await admin.CreateTopicsAsync(topics);
            _logger.LogInformation("Kafka topics verified/created successfully.");
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation("Kafka topics already exist.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Releases startup thread immediately so Kestrel isn't blocked

        await EnsureTopicsExistAsync(stoppingToken);

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true,
            TopicMetadataRefreshIntervalMs = 1000
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions));
            })
            .Build();
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
