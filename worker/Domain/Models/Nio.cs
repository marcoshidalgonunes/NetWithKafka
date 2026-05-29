namespace Transactions.Worker.Domain.Models;

public sealed class Nio
{
    public string? Application { get; set; }
    public string? Terminal { get; set; }
    public DateOnly? Date { get; set; }
    public string? Time { get; set; }
}
