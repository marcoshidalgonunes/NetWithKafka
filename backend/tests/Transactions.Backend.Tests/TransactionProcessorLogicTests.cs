using Moq;
using Transactions.Backend.Models;
using Transactions.Backend.Repositories;
using Transactions.Backend.Services;
using Xunit;

namespace Transactions.Backend.Tests;

public sealed class TransactionProcessorLogicTests
{
    [Fact]
    public async Task ProcessAsync_SetsStatusFromRepository()
    {
        var repo = new Mock<IBalanceRepository>();
        repo.Setup(r => r.ProcessTransactionAsync(1, 2, 10m, It.IsAny<CancellationToken>())).ReturnsAsync("ACCEPTED");

        var sut = new TransactionProcessorLogic(repo.Object);
        var transaction = new Transaction
        {
            AccountId = 1,
            TransactionId = 2,
            Amount = 10m
        };

        var result = await sut.ProcessAsync(transaction, CancellationToken.None);

        Assert.Equal("ACCEPTED", result.Status);
    }
}
