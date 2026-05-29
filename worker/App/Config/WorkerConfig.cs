namespace Transactions.Worker.App.Config;

public sealed class WorkerConfig
{
    public int BatchIntervalMs { get; set; } = 1000;
    public int BatchFlushThreshold { get; set; } = 10;
    public int BatchFlushMinAgeMs { get; set; } = 500;
}
