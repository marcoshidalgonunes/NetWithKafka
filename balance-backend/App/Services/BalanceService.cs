using Balances.Backend.Domain.Contracts;
using Balances.Backend.Domain.Models;
using Balances.Backend.Infrastructure.Repositories;

namespace Balances.Backend.App.Services;

public sealed class BalanceService(BalanceRepository balanceRepository) : IBalance
{
    public async Task<Balance?> ReadAsync(string accountId, CancellationToken cancellationToken = default)
        => await balanceRepository.ReadAsync(accountId, cancellationToken);

    public async Task<bool> UpdateAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default)
        => await balanceRepository.UpdateAsync(accountId, newAmount, cancellationToken);
}
