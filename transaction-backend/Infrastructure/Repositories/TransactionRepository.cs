using System.Data;
using Npgsql;
using NpgsqlTypes;
using Transactions.Backend.Domain.Contracts;
using Transactions.Backend.Domain.Models;

namespace Transactions.Backend.Infrastructure.Repositories;

public sealed class TransactionRepository(IConfiguration configuration, ILogger<TransactionRepository> logger) : ITransaction
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TransactionRepository> _logger = logger;

    public async Task<Transaction?> ReadAsync(string accountId, int transactionId, CancellationToken cancellationToken = default)
    {
        const string sql = "get_transaction";

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
        var statusParam = new NpgsqlParameter
        {
            NpgsqlDbType = NpgsqlDbType.Varchar,
            Size = 10,
            Direction = ParameterDirection.Output
        };

        var parameters = cmd.Parameters;

        parameters.AddWithValue(accountId);
        parameters.AddWithValue(transactionId);
        parameters.Add(amountParam);
        parameters.Add(statusParam);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (amountParam.Value is DBNull || statusParam.Value is DBNull)
            {
                return null;
            }

            return new Transaction
            {
                TransactionId = transactionId,
                Amount = Convert.ToDecimal(amountParam.Value, System.Globalization.CultureInfo.InvariantCulture),
                Status = Convert.ToString(statusParam.Value, System.Globalization.CultureInfo.InvariantCulture)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling get_transaction for accountId={AccountId}, transactionId={TransactionId}", accountId, transactionId);
            return null;
        }
    }

    public async Task<bool> CreateAsync(string accountId, int transactionId, decimal amount, string status, CancellationToken cancellationToken = default)
    {
        const string sql = "create_transaction";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn) { 
            CommandType = CommandType.StoredProcedure 
        };

        var parameters = cmd.Parameters;

        parameters.AddWithValue(accountId);
        parameters.AddWithValue(transactionId);
        parameters.AddWithValue(amount);
        parameters.AddWithValue(status);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling create_transaction for accountId={AccountId}, transactionId={TransactionId}", accountId, transactionId);
            return false;
        }
    }
}
