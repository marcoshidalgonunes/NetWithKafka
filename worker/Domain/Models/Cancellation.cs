namespace Transactions.Worker.Domain.Models;

public sealed class Cancellation
{
    public int? TransactionNumber { get; set; }
    public string? Type { get; set; } = "N";
}
