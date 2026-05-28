using Transactions.Worker.Models;

namespace Transactions.Worker.Clients;

public interface IBalanceClient
{
    Task<Balance?> GetBalanceAsync(string accountId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBalanceAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default);
}
