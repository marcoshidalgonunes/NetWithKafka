namespace Balances.Backend.Infrastructure.Tests.Repositories;

public sealed class BalanceRepositoryTests
{
    private readonly Mock<IBalanceRepository> _repository = new();

    [Fact]
    public async Task GetBalanceAsync_ReturnsBalance_WhenAccountExists()
    {
        // Arrange
        var expected = new Balance { AccountId = "ACC-001", Amount = 1500m, Blocked = false };
        _repository
            .Setup(r => r.GetBalanceAsync("ACC-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.Object.GetBalanceAsync("ACC-001", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ACC-001", result.AccountId);
        Assert.Equal(1500m, result.Amount);
        Assert.False(result.Blocked);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsNull_WhenAccountNotFound()
    {
        // Arrange
        _repository
            .Setup(r => r.GetBalanceAsync("UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Balance?)null);

        // Act
        var result = await _repository.Object.GetBalanceAsync("UNKNOWN", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateBalanceAsync_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        _repository
            .Setup(r => r.UpdateBalanceAsync("ACC-001", 2000m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.Object.UpdateBalanceAsync("ACC-001", 2000m, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateBalanceAsync_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        _repository
            .Setup(r => r.UpdateBalanceAsync("ACC-001", 2000m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.Object.UpdateBalanceAsync("ACC-001", 2000m, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
