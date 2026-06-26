using Transactions.Backend.Domain.Models;

namespace Transactions.Backend.Domain.Contracts;

public interface ITransaction
{
    Task<List<Transaction>?> ReadByDateAsync(string accountId, DateOnly date, CancellationToken cancellationToken = default);

    Task<bool> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
}