namespace Transactions.Worker.Options;

public sealed class WorkerOptions
{
    public int BatchIntervalMs { get; set; } = 1000;
    public int BatchFlushThreshold { get; set; } = 10;
    public int BatchFlushMinAgeMs { get; set; } = 500;
}
