using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Transactions.Worker.App.Config;
using Transactions.Worker.Domain.Contracts;
using Transactions.Worker.Domain.Models;

namespace Transactions.Worker.Infrastructure.Clients;

public sealed class BalanceClient(IHttpClientFactory httpClientFactory, IOptions<BackendConfig> backendOptions, ILogger<BalanceClient> logger) : IBalance
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _baseUrl = backendOptions.Value.BaseUrl.TrimEnd('/');
    private readonly ILogger<BalanceClient> _logger = logger;

    public async Task<Balance?> Get(string accountId, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{_baseUrl}/api/balance/{accountId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Balance>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for accountId={AccountId}", accountId);
            return null;
        }
    }

    public async Task<bool> Put(string accountId, decimal newAmount, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.PutAsJsonAsync($"{_baseUrl}/api/balance/{accountId}", newAmount, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for accountId={AccountId}", accountId);
            return false;
        }
    }
}
