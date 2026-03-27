using Transactions.Worker.Options;
using Transactions.Worker.Repositories;
using Transactions.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

builder.Services.AddSingleton<BalanceRepository>();
builder.Services.AddSingleton<BalanceCalculatorFactory>();
builder.Services.AddHostedService<TransactionProcessorWorker>();

var app = builder.Build();
await app.RunAsync();
