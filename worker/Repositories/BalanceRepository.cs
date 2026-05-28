using System.Data;
using Npgsql;
using NpgsqlTypes;
using Transactions.Worker.Models;

namespace Transactions.Worker.Repositories;

public sealed class BalanceRepository
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
        const string sql = "get_balance";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        var amountParam = new NpgsqlParameter
        {
            NpgsqlDbType = NpgsqlDbType.Numeric,
            Direction = ParameterDirection.Output
        };
        var blockedParam = new NpgsqlParameter
        {
            NpgsqlDbType = NpgsqlDbType.Boolean,
            Direction = ParameterDirection.Output
        };

        var parameters = cmd.Parameters;

        parameters.AddWithValue(accountId);
        parameters.Add(amountParam);
        parameters.Add(blockedParam);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (amountParam.Value is DBNull || blockedParam.Value is DBNull)
            {
                return null;
            }

            return new Balance
            {
                AccountId = accountId,
                Amount = Convert.ToDecimal(amountParam.Value, System.Globalization.CultureInfo.InvariantCulture),
                Blocked = Convert.ToBoolean(blockedParam.Value, System.Globalization.CultureInfo.InvariantCulture)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling get_balance for accountId={AccountId}", accountId);
            return null;
        }
    }

    public async Task<bool> UpdateBalanceAsync(string accountId, decimal newAmount, CancellationToken cancellationToken = default)
    {
        const string sql = "update_balance";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn) { 
            CommandType = CommandType.StoredProcedure 
        };

        var parameters = cmd.Parameters;

        parameters.AddWithValue(accountId);
        parameters.AddWithValue(newAmount);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling update_balance for accountId={AccountId}", accountId);
            return false;
        }
    }
}
