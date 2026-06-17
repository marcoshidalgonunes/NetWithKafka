namespace Transactions.Backend.App.Config;

public sealed class KafkaConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ConsumerTopic { get; set; } = "transaction-received";
    public string ConsumerGroupId { get; set; } = "received-group";
    public string DeadLetterTopic { get; set; } = "transaction-received-dlt";
}
