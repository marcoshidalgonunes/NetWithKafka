using Transactions.Worker.Options;
using Transactions.Worker.Clients;
using Transactions.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
builder.Services.Configure<WorkerOptions>(configuration.GetSection("Worker"));
builder.Services.Configure<BackendOptions>(configuration.GetSection("Backend"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IBalanceClient, BalanceClient>();
builder.Services.AddSingleton<BalanceCalculatorFactory>();
builder.Services.AddHostedService<TransactionProcessorService>();

var app = builder.Build();
await app.RunAsync();
