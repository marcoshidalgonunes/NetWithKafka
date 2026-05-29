namespace Transactions.Worker.Domain.Contracts;

public interface IBalanceCalculator
{
    string Compute(int transactionId, decimal transactionValue);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}
