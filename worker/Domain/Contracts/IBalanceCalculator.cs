namespace Transactions.Worker.Domain.Contracts;

public interface IBalanceCalculator
{
    string Compute(Guid transactionId, decimal transactionValue);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}
