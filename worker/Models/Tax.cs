namespace Transactions.Worker.Models;

public sealed class Tax
{
    public string? Code { get; set; }
    public decimal? Amount { get; set; }
    public string? Concept { get; set; }
}
