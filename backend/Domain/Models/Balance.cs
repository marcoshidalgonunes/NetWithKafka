namespace Transactions.Backend.Domain.Models;

public sealed class Balance
{
    public string? AccountId { get; set; }

    public decimal? Amount { get; set; }

    public bool Blocked { get; set; } = false;
}
