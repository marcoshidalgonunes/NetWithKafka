using Transactions.Backend.Domain.Models;

namespace Transactions.Backend.Domain.Contracts;

public interface IBalance
{
    Task<Balance?> ReadAsync(string accountId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default);
}