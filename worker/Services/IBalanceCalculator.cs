namespace Transactions.Worker.Services;

public interface IBalanceCalculator
{
    string Execute(int transactionId, decimal transactionValue);
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
