using Transactions.Worker.Domain.Contracts;

namespace Transactions.Worker.App.Engine;

public sealed class BalanceBlockedEngine(string status, string accountId, ILogger<BalanceBlockedEngine> logger) : IBalanceCalculator
{
    private readonly string _accountId = accountId;
    private readonly string _status = status;
    private readonly ILogger<BalanceBlockedEngine> _logger = logger;

    public string Compute(int transactionId, decimal transactionValue)
    {
        _logger.LogWarning("Transaction Id '{TransactionId}' was not used for calculation", transactionId);
        return _status;
    }

    public Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("All calculations result for account Id '{AccountId}' was {Status}", _accountId, _status);
        return Task.CompletedTask;
    }
}
