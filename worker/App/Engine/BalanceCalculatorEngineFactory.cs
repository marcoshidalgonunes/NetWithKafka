using Transactions.Worker.Domain.Contracts;

namespace Transactions.Worker.App.Engine;

public sealed class BalanceCalculatorEngineFactory(
    IBalance balanceClient,
    ILogger<BalanceCalculatorEngineFactory> logger,
    ILogger<BalanceBlockedEngine> blockedLogger,
    ILogger<BalanceCalculatorEngine> calculatorLogger)
{
    private readonly IBalance _balanceClient = balanceClient;
    private readonly ILogger<BalanceCalculatorEngineFactory> _logger = logger;
    private readonly ILogger<BalanceBlockedEngine> _blockedLogger = blockedLogger;
    private readonly ILogger<BalanceCalculatorEngine> _calculatorLogger = calculatorLogger;

    public async Task<IBalanceCalculator> CreateAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _balanceClient.Get(accountId, cancellationToken);
            if (balance is null)
            {
                return new BalanceBlockedEngine("INVALID", accountId, _blockedLogger);
            }

            if (balance.Blocked)
            {
                return new BalanceBlockedEngine("BLOCKED", accountId, _blockedLogger);
            }

            return new BalanceCalculatorEngine(_balanceClient, accountId, balance.Amount, _calculatorLogger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for accountId={AccountId}", accountId);
            return new BalanceBlockedEngine("ERROR", accountId, _blockedLogger);
        }
    }
}
