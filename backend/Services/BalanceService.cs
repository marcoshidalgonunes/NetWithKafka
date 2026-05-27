using Transactions.Backend.Models;
using Transactions.Backend.Repositories;

namespace Transactions.Backend.Services;

public sealed class BalanceService(IBalanceRepository balanceRepository)
{
    public async Task<Balance?> GetBalanceAsync(string accountId, CancellationToken cancellationToken = default)
        => await balanceRepository.GetBalanceAsync(accountId, cancellationToken);

    public async Task<bool> UpdateBalanceAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default)
        => await balanceRepository.UpdateBalanceAsync(accountId, newAmount, cancellationToken);
}
