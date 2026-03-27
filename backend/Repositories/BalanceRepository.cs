using Npgsql;

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

    public async Task<string> ProcessTransactionAsync(int accountId, int transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT process_transaction(@p_accountId, @p_transactionId, @p_amount)";

        await using var conn = new NpgsqlConnection(_configuration.GetConnectionString("BankAccounts"));
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p_accountId", accountId);
        cmd.Parameters.AddWithValue("p_transactionId", transactionId);
        cmd.Parameters.AddWithValue("p_amount", amount);

        try
        {
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result?.ToString() ?? "ERROR";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling process_transaction");
            return "ERROR";
        }
    }
}
