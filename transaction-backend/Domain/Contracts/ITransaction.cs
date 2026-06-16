using Transactions.Backend.Domain.Models;

namespace Transactions.Backend.Domain.Contracts;

public interface ITransaction
{
    Task<Transaction?> ReadAsync(string accountId, int transactionId, CancellationToken cancellationToken = default);

    Task<bool> CreateAsync(string accountId, int transactionId, decimal amount, string status, CancellationToken cancellationToken = default);
}