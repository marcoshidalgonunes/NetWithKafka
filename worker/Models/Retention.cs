namespace Transactions.Worker.Models;

public sealed class Retention
{
    public string? Indicator { get; set; } = "00";
    public string? Number { get; set; }
    public string? Code { get; set; }
    public decimal? Amount { get; set; }
    public int? ValidDays { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool? AffectsLiquidation { get; set; } = false;
}
