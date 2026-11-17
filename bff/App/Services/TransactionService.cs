using System.Text.Json;
using Microsoft.Extensions.Options;
using Transactions.Bff.App.Config;
using Transactions.Bff.Domain.Contracts;
using Transactions.Bff.Domain.Models;
using Transactions.Bff.Infrastructure.Kafka;
using Transactions.Bff.Infrastructure.Messaging;
using Transactions.Bff.Infrastructure.Options;

namespace Transactions.Bff.App.Services;

public sealed class TransactionService(
    TransactionProducer producer,
    CorrelationStore store,
    IOptions<KafkaConfig> options,
    ILogger<TransactionService> logger) : ITransaction
{
    private readonly TransactionProducer _producer = producer;
    private readonly CorrelationStore _store = store;
    private readonly KafkaConfig _options = options.Value;
    private readonly ILogger<TransactionService> _logger = logger;

    public async Task<Transaction?> SendAndReceiveAsync(Entry payload, CancellationToken cancellationToken = default)
    {
        Transaction transaction = new()
        {
            TransactionId = Guid.NewGuid(),
            Entry = payload,
            Status = "PROCESS"
        };

        _logger.LogInformation("Producing Transaction to Kafka: {@Payload}", transaction);

        var correlationId = Guid.NewGuid().ToString();
        var tcs = _store.CreatePending(correlationId);
        try
        {
            await _producer.ProduceAsync(correlationId, JsonSerializer.Serialize(transaction, JsonOptions.Serialization), cancellationToken);

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
}
