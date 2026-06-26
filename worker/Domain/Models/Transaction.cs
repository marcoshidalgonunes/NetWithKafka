namespace Transactions.Worker.Domain.Models;

public sealed class Transaction
{
    public required Guid TransactionId { get; set; }

    public required Entry Entry { get; set; }

    public required string Status { get; set; }
}
