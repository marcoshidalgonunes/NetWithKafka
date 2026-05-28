using System.Collections.Concurrent;
using Transactions.Bff.Domain.Models;

namespace Transactions.Bff.Infrastructure.Kafka;

public sealed class CorrelationStore
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<Transaction>> _pending = new();

    public TaskCompletionSource<Transaction> CreatePending(string correlationId)
    {
        var tcs = new TaskCompletionSource<Transaction>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[correlationId] = tcs;
        return tcs;
    }

    public bool TryComplete(string correlationId, Transaction transaction)
    {
        if (_pending.TryRemove(correlationId, out var tcs))
        {
            return tcs.TrySetResult(transaction);
        }

        return false;
    }

    public void Remove(string correlationId)
    {
        _pending.TryRemove(correlationId, out _);
    }
}
