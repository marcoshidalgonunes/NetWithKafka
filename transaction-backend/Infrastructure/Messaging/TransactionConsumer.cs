using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Transactions.Backend.App.Config;
using Transactions.Backend.App.Services;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.Infrastructure.Kafka;
using Transactions.Backend.Infrastructure.Options;

namespace Transactions.Backend.Infrastructure.Messaging;

public sealed class TransactionConsumer(
    TransactionService transactionService, 
    TransactionConsumerBuilder consumerBuilder,
    IOptions<KafkaConfig> kafkaOptions,
    ILogger<TransactionConsumer> logger) : BackgroundService
{
    private readonly TransactionService _transactionService = transactionService;
    private readonly TransactionConsumerBuilder _consumerBuilder = consumerBuilder;
    private readonly KafkaConfig _config = kafkaOptions.Value;
    private readonly ILogger<TransactionConsumer> _logger = logger;
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {

        using var consumer = await _consumerBuilder.Build();
        consumer.Subscribe(_config.ConsumerTopic);
        
        _logger.LogInformation("Transaction Backend consuming from topic {Topic}", _config.ConsumerTopic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                if (consumeResult != null)
                {
                    _logger.LogInformation("Received message: {Message}", consumeResult.Message.Value);

                    var transaction = JsonSerializer.Deserialize<Transaction>(consumeResult.Message.Value, JsonOptions.Serialization);
                    if (transaction != null && transaction.Entry.Account != null)
                    {
                        await _transactionService.CreateAsync(transaction, cancellationToken);
                    }
                }                
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning("Topic not ready yet, retrying in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }            
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Transaction Consumer is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message: {ErrorMessage}", ex.Message);
            } 
        }
    }
}