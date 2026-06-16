using Transactions.Backend.Domain.Contracts;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.Infrastructure.Repositories;

namespace Transactions.Backend.App.Services;

public sealed class TransactionService(TransactionRepository transactionRepository) : ITransaction
{
    public async Task<Transaction?> ReadAsync(string accountId, int transactionId, CancellationToken cancellationToken = default)
        => await transactionRepository.ReadAsync(accountId, transactionId, cancellationToken);

    public async Task<bool> CreateAsync(string accountId, int transactionId, decimal amount, string status, CancellationToken cancellationToken = default)
        => await transactionRepository.CreateAsync(accountId, transactionId, amount, status, cancellationToken);
}
