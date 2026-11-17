using System.Text.Json.Serialization;
using Transactions.Backend.Infrastructure.Repositories;
using Transactions.Backend.App.Services;
using Transactions.Backend.App.Config;
using Transactions.Backend.Infrastructure.Messaging;
using Transactions.Backend.Infrastructure.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var services = builder.Services;
services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));

services.AddSingleton<TransactionRepository>();
services.AddSingleton<TransactionConsumerBuilder>();
services.AddSingleton<TransactionConsumer>();
services.AddSingleton<TransactionService>();

services.AddHostedService(sp =>
{
    // TransactionService subscribes to the consumer's event in its constructor.
    _ = sp.GetRequiredService<TransactionService>();
    return sp.GetRequiredService<TransactionConsumer>();
});

services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain";

        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var message = exceptionFeature?.Error?.Message ?? "Unknown error";
        await context.Response.WriteAsync($"Unexpected error: {message}");
    });
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/actuator/health", () => Results.Ok(new { status = "UP" }));

app.Run();
