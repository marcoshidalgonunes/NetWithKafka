using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using Transactions.Backend.App.Config;

namespace Transactions.Backend.Infrastructure.Kafka;

public sealed class TransactionConsumerBuilder(
    IOptions<KafkaConfig> options,
    ILogger<TransactionConsumerBuilder> logger)
{
    private readonly KafkaConfig _config = options.Value;
    private readonly ILogger<TransactionConsumerBuilder> _logger = logger;

    private async Task EnsureTopicsExistAsync()
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = _config.BootstrapServers
        }).Build();

        var desiredTopics = new[]
        {
            new TopicSpecification { Name = _config.ConsumerTopic, NumPartitions = 3, ReplicationFactor = 1 },
        };

        var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

        var existingTopics = metadata.Topics
            .Where(t => t.Error.Code == ErrorCode.NoError)
            .Select(t => t.Topic)
            .ToHashSet(StringComparer.Ordinal);

        var missingTopics = desiredTopics
            .Where(t => !existingTopics.Contains(t.Name))
            .ToList();

        if (missingTopics.Count == 0)
        {
            _logger.LogInformation("Kafka topics already exist.");
            return;
        }

        try
        {
            await admin.CreateTopicsAsync(missingTopics);
            _logger.LogInformation("Kafka topics verified/created successfully.");
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation("Kafka topics already exist.");
        }
    }    
	
	public async Task<IConsumer<string, string>> Build() 
	{
        await Task.Yield(); // Releases startup thread immediately so Kestrel isn't blocked

        await EnsureTopicsExistAsync();

        var config = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true,
            TopicMetadataRefreshIntervalMs = 1000
        };

        return new ConsumerBuilder<string, string>(config)
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions));
            })
            .Build();
	}		
}			

