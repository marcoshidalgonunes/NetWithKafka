using Transactions.Bff.Models;

namespace Transactions.Bff.Services;

public interface ITransactionService
{
    Task<Transaction?> SendAndReceiveAsync(Transaction payload, CancellationToken cancellationToken = default);
}
