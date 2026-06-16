namespace Transactions.Backend.Domain.Models;

public sealed class Transaction
{
    public int? TransactionId { get; set; }
    public Account? Account { get; set; }
    public Origin? Origin { get; set; }
    public string? Type { get; set; }
    public string? Code { get; set; }
    public decimal? Amount { get; set; }
    public string? Concept { get; set; }
    public string? Timestamp { get; set; }
    public string? AccountingDate { get; set; }
    public string? CheckNumber { get; set; }
    public string? InternalReference { get; set; }
    public string? Observation { get; set; }
    public string? HistoryComplement { get; set; }
    public Tax? Tax { get; set; }
    public Controls? Controls { get; set; }
    public Cancellation? Cancellation { get; set; }
    public Retention? Retention { get; set; }
    public Nio? Nio { get; set; }
    public string? Status { get; set; }
}
