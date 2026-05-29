using System.Text.Json.Serialization;
using Confluent.Kafka;
using Transactions.Bff.App.Config;
using Transactions.Bff.Domain.Contracts;
using Transactions.Bff.Infrastructure.Kafka;
using Transactions.Bff.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<CorrelationStore>();
builder.Services.AddSingleton<ITransaction, TransactionMessaging>();
builder.Services.AddHostedService<ReplyConsumerService>();

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaConfig>>().Value;
    var config = new ProducerConfig
    {
        BootstrapServers = options.BootstrapServers,
        Acks = Acks.All
    };
    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHealthChecks();

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
