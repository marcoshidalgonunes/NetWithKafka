using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Bff.App.Config;
using Transactions.Bff.Domain.Contracts;
using Transactions.Bff.Domain.Models;
using Transactions.Bff.Infrastructure.Kafka;
using Transactions.Bff.Infrastructure.Options;

namespace Transactions.Bff.Infrastructure.Messaging;

public sealed class TransactionMessaging(
    IProducer<string, string> producer,
    CorrelationStore store,
    IOptions<KafkaConfig> options,
    ILogger<TransactionMessaging> logger) : ITransaction
{
    private readonly IProducer<string, string> _producer = producer;
    private readonly CorrelationStore _store = store;
    private readonly KafkaConfig _options = options.Value;
    private readonly ILogger<TransactionMessaging> _logger = logger;

    public async Task<Transaction?> SendAndReceiveAsync(Transaction payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Producing Transaction to Kafka: {@Payload}", payload);

        var correlationId = Guid.NewGuid().ToString();
        var tcs = _store.CreatePending(correlationId);
        var kafkaMessage = new Message<string, string>
        {
            Key = correlationId,
            Value = JsonSerializer.Serialize(payload, JsonOptions.Serialization),
            Headers = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes(correlationId) },
                { "consumerTopic", Encoding.UTF8.GetBytes(_options.ConsumerTopic) }
            }
        };

        try
        {
            await ProduceWithRetryAsync(_options.ProducerTopic, kafkaMessage, cancellationToken);

            using var timeoutCts = new CancellationTokenSource(_options.TimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            using var registration = linkedCts.Token.Register(() => tcs.TrySetCanceled(linkedCts.Token));

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout while waiting for reply");
            _store.Remove(correlationId);
            throw new TimeoutException("Timeout while waiting for Kafka reply", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while waiting for reply");
            _store.Remove(correlationId);
            return null;
        }
    }

    private async Task ProduceWithRetryAsync(string topic, Message<string, string> message, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _options.RetryMaxAttempts; attempt++)
        {
            try
            {
                await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (ProduceException<string, string> ex) when (attempt < _options.RetryMaxAttempts)
            {
                _logger.LogWarning(ex, "Kafka send failed, retrying {Attempt}/{MaxAttempts}", attempt, _options.RetryMaxAttempts);
                await Task.Delay(_options.RetryBackoffMs, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka send failed");
                throw;
            }
        }
    }
}
