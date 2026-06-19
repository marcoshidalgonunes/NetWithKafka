using Moq;
using Microsoft.Extensions.Logging;
using Transactions.Worker.Domain.Models;
using Xunit;
using Transactions.Worker.Domain.Contracts;
using Transactions.Worker.App.Engine;

namespace Transactions.Worker.Tests;

public sealed class BalanceCalculatorTests
{
    private static IBalance CreateRepository(decimal currentAmount)
    {
        var mock = new Mock<IBalance>();
        mock.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Balance { Amount = currentAmount });
        return mock.Object;
    }

    [Fact]
    public void Execute_ReturnsRejected_WhenBalanceWouldBeNegative()
    {
        var repo = CreateRepository(currentAmount: 10m);
        var logger = Mock.Of<ILogger<BalanceCalculatorEngine>>();
        var sut = new BalanceCalculatorEngine(repo, "123456", 10m, logger);

        var status = sut.Compute(1, -20m);

        Assert.Equal("REJECTED", status);
    }

    [Fact]
    public void Execute_ReturnsAccepted_WhenBalanceIsNonNegative()
    {
        var repo = CreateRepository(currentAmount: 10m);
        var logger = Mock.Of<ILogger<BalanceCalculatorEngine>>();
        var sut = new BalanceCalculatorEngine(repo, "123456", 10m, logger);

        var status = sut.Compute(1, 5m);

        Assert.Equal("ACCEPTED", status);
    }
}
