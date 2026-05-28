using Microsoft.Extensions.Logging;
using Moq;
using Transactions.Worker.Clients;
using Transactions.Worker.Services;
using Xunit;

namespace Transactions.Worker.Tests;

public sealed class BalanceCalculatorTests
{
    private static BalanceClient CreateRepository()
    {
        var config = Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = Mock.Of<ILogger<BalanceClient>>();
        return new BalanceRepository(config, logger);
    }

    [Fact]
    public void Execute_ReturnsRejected_WhenBalanceWouldBeNegative()
    {
        var repo = CreateRepository();
        var logger = Mock.Of<ILogger<BalanceCalculator>>();
        var sut = new BalanceCalculator(repo, "123456", 10m, logger);

        var status = sut.Execute(1, -20m);

        Assert.Equal("REJECTED", status);
    }

    [Fact]
    public void Execute_ReturnsAccepted_WhenBalanceIsNonNegative()
    {
        var repo = CreateRepository();
        var logger = Mock.Of<ILogger<BalanceCalculator>>();
        var sut = new BalanceCalculator(repo, "123456", 10m, logger);

        var status = sut.Execute(1, 5m);

        Assert.Equal("ACCEPTED", status);
    }
}
