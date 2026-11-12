using Transactions.Backend.Domain.Contracts;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.Infrastructure.Messaging;
using Transactions.Backend.Infrastructure.Repositories;

namespace Transactions.Backend.App.Services;

public sealed class TransactionService : ITransaction
{
    private readonly TransactionRepository _transactionRepository;

    public TransactionService(
        TransactionRepository transactionRepository,
        TransactionConsumer transactionConsumer)
    {
        _transactionRepository = transactionRepository;
        transactionConsumer.TransactionReceived += HandleTransactionReceivedAsync;
    }

    private Task<bool> HandleTransactionReceivedAsync(Transaction transaction, CancellationToken cancellationToken)
        => CreateAsync(transaction, cancellationToken);

    public async Task<List<Transaction>?> ReadByDateAsync(string accountId, DateOnly date, CancellationToken cancellationToken = default)
        => await _transactionRepository.ReadByDateAsync(accountId, date, cancellationToken);

    public async Task<bool> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => await _transactionRepository.CreateAsync(transaction, cancellationToken);
}