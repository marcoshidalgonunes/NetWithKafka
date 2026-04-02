using Transactions.Worker.Options;
using Transactions.Worker.Repositories;
using Transactions.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
builder.Services.Configure<WorkerOptions>(configuration.GetSection("Worker"));

builder.Services.AddSingleton<BalanceRepository>();
builder.Services.AddSingleton<BalanceCalculatorFactory>();
builder.Services.AddHostedService<TransactionProcessorWorker>();

var app = builder.Build();
await app.RunAsync();
