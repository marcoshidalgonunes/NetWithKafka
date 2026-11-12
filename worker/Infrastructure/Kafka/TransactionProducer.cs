using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Worker.App.Config;

namespace Transactions.Worker.Infrastructure.Kafka;

public sealed record OutgoingMessage(
    string Key,
    string Value,
    IReadOnlyDictionary<string, byte[]> Headers);

public sealed class KafkaProducerException(string message, Exception innerException)
    : Exception(message, innerException);

public sealed class TransactionProducer(
    IOptions<KafkaConfig> kafkaOptions
) : IDisposable
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = kafkaOptions.Value.BootstrapServers,
        AllowAutoCreateTopics = true,
        Acks = Acks.All,
        MessageTimeoutMs = kafkaOptions.Value.ProduceTimeoutMs
    }).Build();

    public async Task ProduceAsync(string topic, OutgoingMessage outgoingMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = outgoingMessage.Key,
                Value = outgoingMessage.Value,
                Headers = []
            };

            foreach (var header in outgoingMessage.Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }

            await _producer.ProduceAsync(topic, message, cancellationToken);
        }
        catch (ProduceException<string, string> ex)
        {
            throw new KafkaProducerException("Kafka send failed", ex);
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
