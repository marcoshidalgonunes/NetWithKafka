using Transactions.Worker.Domain.Contracts;

namespace Transactions.Worker.App.Engine;

public sealed class BalanceCalculatorEngine(IBalance balanceClient, string accountId, decimal balance, ILogger<BalanceCalculatorEngine> logger) : IBalanceCalculator
{
    private readonly IBalance _balanceClient = balanceClient;
    private readonly string _accountId = accountId;
    private readonly ILogger<BalanceCalculatorEngine> _logger = logger;
    private decimal _balance = balance;

    public string Compute(int transactionId, decimal transactionValue)
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
            await _balanceClient.Put(_accountId, _balance, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for account Id {AccountId}", _accountId);
        }
    }
}
