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

    public async Task<List<Transaction>?> ReadByDateAsync(string accountId, DateOnly date, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM get_transactions_by_date(@p_accountId, @p_date)";
        List<Transaction> transactions = [];

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);

        var parameters = cmd.Parameters;
        parameters.AddWithValue("p_accountId", accountId);
        parameters.AddWithValue("p_date", NpgsqlDbType.Date, date);

        try
        {
            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var ordTransactionId = reader.GetOrdinal("transactionId");
            var ordAmount = reader.GetOrdinal("amount");
            var ordCode = reader.GetOrdinal("code");
            var ordDescription = reader.GetOrdinal("description");
            var ordStatus = reader.GetOrdinal("status");
            var ordCreatedTimestamp = reader.GetOrdinal("createdTimestamp");

            while (await reader.ReadAsync(cancellationToken))
            {
                var transactionId = reader.GetGuid(ordTransactionId);
                var amount = reader.GetDecimal(ordAmount);
                var code = reader.GetString(ordCode);
                var description = reader.GetString(ordDescription);
                var status = reader.GetString(ordStatus);
                var createdTimestamp = reader.GetDateTime(ordCreatedTimestamp);

                var entry = new Entry
                {
                    Amount = amount,
                    Code = code,
                    Description = description,
                    CreatedTimestamp = createdTimestamp
                };

                var transaction = new Transaction
                {
                    TransactionId = transactionId,
                    Entry = entry,
                    Status = status
                };

                transactions.Add(transaction);
            }

            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling get_transactions_by_date for accountId={AccountId}, date={Date}", accountId, date);
            return null;
        }
    }

    public async Task<bool> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = "create_transaction";
        var entry = transaction.Entry;
        var accountId = entry.Account?.ToAccountId()!;

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn) { 
            CommandType = CommandType.StoredProcedure 
        };

        var parameters = cmd.Parameters;

        parameters.AddWithValue(transaction.TransactionId);
        parameters.AddWithValue(accountId);
        parameters.AddWithValue(entry.Amount);
        parameters.AddWithValue(entry.Code);
        parameters.AddWithValue(entry.Description);
        parameters.AddWithValue(transaction.Status);
        parameters.AddWithValue(entry.CreatedTimestamp);   

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling create_transaction for accountId={AccountId}, transactionId={TransactionId}", accountId, transaction.TransactionId);
            return false;
        }
    }
}
