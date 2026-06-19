using Transactions.Worker.App.Config;
using Transactions.Worker.Infrastructure.Clients;
using Transactions.Worker.Domain.Contracts;
using Transactions.Worker.App.Engine;
using Transactions.Worker.App.Service;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.Configure<KafkaConfig>(configuration.GetSection("Kafka"));
services.Configure<WorkerConfig>(configuration.GetSection("Worker"));
services.Configure<BackendConfig>(configuration.GetSection("Backend"));

services.AddHttpClient();
services.AddSingleton<IBalance, BalanceClient>();
services.AddSingleton<BalanceCalculatorEngineFactory>();
services.AddHostedService<TransactionService>();

var app = builder.Build();
await app.RunAsync();
