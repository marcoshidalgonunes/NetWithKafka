using Transactions.Bff.Domain.Models;
using Transactions.Bff.Services;
using Xunit;

namespace Transactions.Bff.Tests;

public sealed class KafkaCorrelationStoreTests
{
    [Fact]
    public async Task TryComplete_CompletesPendingTask()
    {
        var store = new KafkaCorrelationStore();
        var tcs = store.CreatePending("corr-1");
        var transaction = new Transaction { Status = "DONE" };

        var completed = store.TryComplete("corr-1", transaction);

        Assert.True(completed);
        var result = await tcs.Task;
        Assert.Equal("DONE", result.Status);
    }
}
