namespace Transactions.Backend.Repositories;

public interface IBalanceRepository
{
    Task<string> ProcessTransactionAsync(int accountId, int transactionId, decimal amount, CancellationToken cancellationToken = default);
}
