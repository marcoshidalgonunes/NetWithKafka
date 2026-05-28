namespace Transactions.Bff.App.Config;

public sealed class KafkaConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ProducerTopic { get; set; } = "transaction-received";
    public string ConsumerTopic { get; set; } = "transaction-processed";
    public string ConsumerGroupId { get; set; } = "processed-group";
    public string DeadLetterTopic { get; set; } = "transaction-processed-dlt";
    public int TimeoutMs { get; set; } = 10000;
    public int RetryMaxAttempts { get; set; } = 3;
    public int RetryBackoffMs { get; set; } = 2000;
}
