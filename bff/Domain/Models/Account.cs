namespace Transactions.Bff.Domain.Models;

public sealed class Account
{
    public required string Branch { get; set; }
    public required string Number { get; set; }
}
