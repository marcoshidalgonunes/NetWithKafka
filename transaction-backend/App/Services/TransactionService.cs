using Transactions.Backend.Domain.Contracts;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.Infrastructure.Repositories;

namespace Transactions.Backend.App.Services;

public sealed class TransactionService(TransactionRepository transactionRepository) : ITransaction
{
    public async Task<List<Transaction>?> ReadByDateAsync(string accountId, DateOnly date, CancellationToken cancellationToken = default)
        => await transactionRepository.ReadByDateAsync(accountId, date, cancellationToken);

    public async Task<bool> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => await transactionRepository.CreateAsync(transaction, cancellationToken);
}
