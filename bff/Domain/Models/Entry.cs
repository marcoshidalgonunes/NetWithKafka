namespace Transactions.Bff.Domain.Models;

public sealed class Entry
{
    public required Account Account { get; set; }
    
    public required string Code { get; set; }

    public required string Description { get; set; }

    public required decimal Amount { get; set; }

    public required DateTime CreatedTimestamp { get; set; }
}