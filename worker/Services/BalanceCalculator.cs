using Transactions.Worker.Clients;

namespace Transactions.Worker.Services;

public sealed class BalanceCalculator(IBalanceClient balanceClient, string accountId, decimal balance, ILogger<BalanceCalculator> logger) : IBalanceCalculator
{
    private readonly IBalanceClient _balanceClient = balanceClient;
    private readonly string _accountId = accountId;
    private readonly ILogger<BalanceCalculator> _logger = logger;
    private decimal _balance = balance;

    public string Execute(int transactionId, decimal transactionValue)
    {
        var updatedBalance = _balance + transactionValue;
        if (updatedBalance < 0)
        {
            _logger.LogWarning("Transaction Id '{TransactionId}' resulted in negative balance: {UpdatedBalance}", transactionId, updatedBalance);
            return "REJECTED";
        }

        _balance = updatedBalance;
        _logger.LogInformation("Transaction Id '{TransactionId}' updated balance = {Balance}", transactionId, _balance);
        return "ACCEPTED";
    }

    public async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating balance for account Id '{AccountId}' to {Balance}", _accountId, _balance);
        try
        {
            await _balanceClient.UpdateBalanceAsync(_accountId, _balance, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for account Id {AccountId}", _accountId);
        }
    }
}
