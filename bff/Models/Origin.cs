namespace Transactions.Bff.Models;

public sealed class Origin
{
    public string? Entity { get; set; }
    public string? Branch { get; set; }
    public string? UserId { get; set; }
    public string? Cashier { get; set; }
    public string? Terminal { get; set; }
    public string? Channel { get; set; }
    public bool? IsAccounting { get; set; } = false;
}
