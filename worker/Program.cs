using Transactions.Worker.App.Config;
using Transactions.Worker.Infrastructure.Clients;
using Transactions.Worker.Domain.Contracts;
using Transactions.Worker.App.Engine;
using Transactions.Worker.App.Service;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<KafkaConfig>(configuration.GetSection("Kafka"));
builder.Services.Configure<WorkerConfig>(configuration.GetSection("Worker"));
builder.Services.Configure<BackendConfig>(configuration.GetSection("Backend"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IBalance, BalanceClient>();
builder.Services.AddSingleton<BalanceCalculatorEngineFactory>();
builder.Services.AddHostedService<TransactionService>();

var app = builder.Build();
await app.RunAsync();
