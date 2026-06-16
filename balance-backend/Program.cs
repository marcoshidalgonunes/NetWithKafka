using System.Text.Json.Serialization;
using Balances.Backend.Infrastructure.Repositories;
using Balances.Backend.App.Services;
using Balances.Backend.Domain.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddSingleton<BalanceRepository>();
builder.Services.AddSingleton<BalanceService>();

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
