namespace Transactions.Worker.Domain.Models;

public sealed class Account
{
    public string? Entity { get; set; }
    public string? Branch { get; set; }
    public string? Number { get; set; }

    public string ToAccountId()
    {
        return $"{Branch}{Number}";
    }
}
