using Confluent.Kafka;
using Transactions.Backend.Options;
using Transactions.Backend.Repositories;
using Transactions.Backend.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IBalanceRepository, BalanceRepository>();
builder.Services.AddSingleton<TransactionProcessorLogic>();
builder.Services.AddHostedService<TransactionProcessorWorker>();

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaOptions>>().Value;
    var config = new ProducerConfig
    {
        BootstrapServers = options.BootstrapServers,
        Acks = Acks.All,
        MessageTimeoutMs = options.ProduceTimeoutMs
    };
    return new ProducerBuilder<string, string>(config).Build();
});

var app = builder.Build();
await app.RunAsync();
