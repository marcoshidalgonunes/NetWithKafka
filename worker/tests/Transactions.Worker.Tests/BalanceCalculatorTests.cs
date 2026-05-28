using Moq;
using Microsoft.Extensions.Logging;
using Transactions.Worker.Clients;
using Transactions.Worker.Models;
using Transactions.Worker.Services;
using Xunit;

namespace Transactions.Worker.Tests;

public sealed class BalanceCalculatorTests
{
    private static IBalanceClient CreateRepository(decimal currentAmount)
    {
        var mock = new Mock<IBalanceClient>();
        mock.Setup(r => r.GetBalanceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Balance { Amount = currentAmount });
        return mock.Object;
    }

    [Fact]
    public void Execute_ReturnsRejected_WhenBalanceWouldBeNegative()
    {
        var repo = CreateRepository(currentAmount: 10m);
        var logger = Mock.Of<ILogger<BalanceCalculator>>();
        var sut = new BalanceCalculator(repo, "123456", 10m, logger);

        var status = sut.Execute(1, -20m);

        Assert.Equal("REJECTED", status);
    }

    [Fact]
    public void Execute_ReturnsAccepted_WhenBalanceIsNonNegative()
    {
        var repo = CreateRepository(currentAmount: 10m);
        var logger = Mock.Of<ILogger<BalanceCalculator>>();
        var sut = new BalanceCalculator(repo, "123456", 10m, logger);

        var status = sut.Execute(1, 5m);

        Assert.Equal("ACCEPTED", status);
    }
}
