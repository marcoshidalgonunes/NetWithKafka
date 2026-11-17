using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Bff.App.Config;

namespace Transactions.Bff.Infrastructure.Messaging;

public sealed class KafkaProducerException(string message, Exception innerException)
    : Exception(message, innerException);

public sealed class TransactionProducer(
    IOptions<KafkaConfig> options,
    ILogger<TransactionProducer> logger
) : IDisposable
{
    private readonly KafkaConfig _config = options.Value;
    private readonly ILogger<TransactionProducer> _logger = logger;

    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = options.Value.BootstrapServers,
        AllowAutoCreateTopics = true,
        Acks = Acks.All,
        MessageTimeoutMs = options.Value.TimeoutMs
    }).Build();

    public async Task ProduceAsync(string correlationId, string value, CancellationToken cancellationToken = default)
    {
		var message = new Message<string, string>
		{
			Key = correlationId,
			Value = value,
            Headers = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes(correlationId) },
                { "consumerTopic", Encoding.UTF8.GetBytes(_config.ConsumerTopic) }
            }
		};

		for (var attempt = 1; attempt <= _config.RetryMaxAttempts; attempt++)
		{
			try
			{
				await _producer.ProduceAsync(_config.ProducerTopic, message, cancellationToken).ConfigureAwait(false);
				return;
			}
			catch (ProduceException<string, string> ex) when (attempt < _config.RetryMaxAttempts)
			{
				_logger.LogWarning(ex, "Kafka send failed, retrying {Attempt}/{MaxAttempts}", attempt, _config.RetryMaxAttempts);
				await Task.Delay(_config.RetryBackoffMs, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Kafka send failed");
				throw;
			}
		}
	}

    public void Dispose()
    {
        _producer.Dispose();
    }
}
