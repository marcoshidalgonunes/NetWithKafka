using Moq;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.Domain.Contracts;
using Xunit;

namespace Transactions.Backend.Infrastructure.Repositories.Tests;

public sealed class TransactionRepositoryTests
{
    private readonly Mock<ITransaction> _repository = new();

    [Fact]
    public async Task ReadAsync_ReturnsTransaction_WhenFound()
    {
        // Arrange
        var expected = new Transaction { TransactionId = 42, Amount = 250.50m, Status = "completed" };
        _repository
            .Setup(r => r.ReadByDateAsync("ACC-001", 42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.Object.ReadByDateAsync("ACC-001", 42, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.TransactionId);
        Assert.Equal(250.50m, result.Amount);
        Assert.Equal("completed", result.Status);
    }

    [Fact]
    public async Task ReadAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repository
            .Setup(r => r.ReadByDateAsync("ACC-001", 999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _repository.Object.ReadByDateAsync("ACC-001", 999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsTrue_WhenCreationSucceeds()
    {
        // Arrange
        _repository
            .Setup(r => r.CreateAsync("ACC-001", 42, 250.50m, "pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.Object.CreateAsync("ACC-001", 42, 250.50m, "pending", CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsFalse_WhenCreationFails()
    {
        // Arrange
        _repository
            .Setup(r => r.CreateAsync("ACC-001", 42, 250.50m, "pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.Object.CreateAsync("ACC-001", 42, 250.50m, "pending", CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
