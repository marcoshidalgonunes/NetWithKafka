using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Worker.App.Config;

namespace Transactions.Worker.Infrastructure.Kafka;

public sealed class KafkaProducerException(string message, Exception innerException)
    : Exception(message, innerException);

public sealed class TransactionProducer(
    IOptions<KafkaConfig> options
) : IDisposable
{
    private readonly KafkaConfig _config = options.Value;
  
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = options.Value.BootstrapServers,
        AllowAutoCreateTopics = true,
        Acks = Acks.All,
        MessageTimeoutMs = options.Value.ProduceTimeoutMs
    }).Build();

    public async Task ProduceAsync(string correlationId, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = correlationId,
                Value = value,
                Headers = new Headers
                {
                    { "correlationId", Encoding.UTF8.GetBytes(correlationId) }
                }
            };

            await _producer.ProduceAsync(_config.ProducerTopic, message, cancellationToken);
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
