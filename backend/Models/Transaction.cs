namespace Transactions.Backend.Models;

public sealed class Transaction
{
    public int? AccountId { get; set; }
    public int? TransactionId { get; set; }
    public decimal? Amount { get; set; }
    public string? Status { get; set; }
}
