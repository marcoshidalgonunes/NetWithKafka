using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Worker.App.Config;

namespace Transactions.Worker.Infrastructure.Messaging;

public sealed record ConsumedMessage(
    string? Key,
    string Value,
    IReadOnlyDictionary<string, byte[]> Headers);

public sealed class ConsumerException(string message, Exception innerException)
    : Exception(message, innerException);

public sealed class TransactionConsumer(
    IOptions<KafkaConfig> options
) : IDisposable
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
    {
        BootstrapServers = options.Value.BootstrapServers,
        GroupId = options.Value.ConsumerGroupId,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    }).Build();

    public void Subscribe(string topic)
        => _consumer.Subscribe(topic);

    public ConsumedMessage? Consume(TimeSpan timeout)
    {
        try
        {
            var record = _consumer.Consume(timeout);
            if (record?.Message is null)
            {
                return null;
            }

            var headers = new Dictionary<string, byte[]>();
            if (record.Message.Headers is not null)
            {
                foreach (var header in record.Message.Headers)
                {
                    headers[header.Key] = header.GetValueBytes();
                }
            }

            return new ConsumedMessage(
                record.Message.Key,
                record.Message.Value,
                headers);
        }
        catch (ConsumeException ex)
        {
            throw new ConsumerException("Kafka consume error", ex);
        }
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }
}
