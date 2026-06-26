using Transactions.Bff.Domain.Models;

namespace Transactions.Bff.Domain.Contracts;

public interface ITransaction
{
    Task<Transaction?> SendAndReceiveAsync(Entry payload, CancellationToken cancellationToken = default);
}
