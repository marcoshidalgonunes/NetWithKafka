namespace Transactions.Worker.Models;

public sealed class Balance
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool Blocked { get; set; }
}
