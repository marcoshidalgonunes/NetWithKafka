using System.Data;
using Npgsql;
using NpgsqlTypes;
using Transactions.Backend.Models;

namespace Transactions.Backend.Repositories;

public sealed class BalanceRepository : IBalanceRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BalanceRepository> _logger;

    public BalanceRepository(IConfiguration configuration, ILogger<BalanceRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Balance?> GetBalanceAsync(string accountId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand("get_balance", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("p_accountId", accountId);

        var amountParam = new NpgsqlParameter("p_amount", NpgsqlDbType.Numeric) { Direction = ParameterDirection.InputOutput, Value = 0m };
        var blockedParam = new NpgsqlParameter("p_blocked", NpgsqlDbType.Boolean) { Direction = ParameterDirection.InputOutput, Value = false };

        cmd.Parameters.Add(amountParam);
        cmd.Parameters.Add(blockedParam);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return new Balance
            {
                AccountId = accountId,
                Amount = (decimal)amountParam.Value!,
                Blocked = (bool)blockedParam.Value!
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling get_balance for accountId {AccountId}", accountId);
            return null;
        }
    }

    public async Task<bool> UpdateBalanceAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand("update_balance", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("p_accountId", accountId);
        cmd.Parameters.AddWithValue("p_newAmount", newAmount);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling update_balance for accountId {AccountId}", accountId);
            return false;
        }
    }
}