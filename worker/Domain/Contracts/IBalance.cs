using Transactions.Worker.Domain.Models;

namespace Transactions.Worker.Domain.Contracts;

public interface IBalance
{
    Task<Balance?> Get(string accountId, CancellationToken cancellationToken = default);
    Task<bool> Put(string accountId, decimal newAmount, CancellationToken cancellationToken = default);
}
