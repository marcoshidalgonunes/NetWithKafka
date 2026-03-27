using Npgsql;
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
        const string sql = "CALL get_balance(@p_accountId, @p_amount, @p_blocked)";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_accountId", accountId);

        var amountParam = new NpgsqlParameter("p_amount", NpgsqlTypes.NpgsqlDbType.Numeric)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        var blockedParam = new NpgsqlParameter("p_blocked", NpgsqlTypes.NpgsqlDbType.Boolean)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        cmd.Parameters.Add(amountParam);
        cmd.Parameters.Add(blockedParam);

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
        const string sql = "CALL update_balance(@p_accountId, @p_newAmount)";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_accountId", accountId);
        cmd.Parameters.AddWithValue("p_newAmount", newAmount);

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
