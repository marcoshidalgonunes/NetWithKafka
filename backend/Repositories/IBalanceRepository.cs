using Transactions.Backend.Models;

namespace Transactions.Backend.Repositories;

public interface IBalanceRepository
{
    Task<Balance?> GetBalanceAsync(string accountId, CancellationToken cancellationToken = default);

    Task<bool> UpdateBalanceAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default);
}
