using Transactions.Backend.Models;
using Transactions.Backend.Repositories;

namespace Transactions.Backend.Services;

public sealed class TransactionProcessorLogic
{
    private readonly IBalanceRepository _balanceRepository;

    public TransactionProcessorLogic(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<Transaction> ProcessAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var status = await _balanceRepository.ProcessTransactionAsync(
            transaction.AccountId ?? 0,
            transaction.TransactionId ?? 0,
            transaction.Amount ?? 0m,
            cancellationToken);

        transaction.Status = status;
        return transaction;
    }
}
