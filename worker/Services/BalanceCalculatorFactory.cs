using Transactions.Worker.Clients;

namespace Transactions.Worker.Services;

public sealed class BalanceCalculatorFactory
{
    private readonly IBalanceClient _balanceClient;
    private readonly ILogger<BalanceCalculatorFactory> _logger;
    private readonly ILogger<BalanceBlocked> _blockedLogger;
    private readonly ILogger<BalanceCalculator> _calculatorLogger;

    public BalanceCalculatorFactory(
        IBalanceClient balanceClient,
        ILogger<BalanceCalculatorFactory> logger,
        ILogger<BalanceBlocked> blockedLogger,
        ILogger<BalanceCalculator> calculatorLogger)
    {
        _balanceClient = balanceClient;
        _logger = logger;
        _blockedLogger = blockedLogger;
        _calculatorLogger = calculatorLogger;
    }

    public async Task<IBalanceCalculator> CreateAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _balanceClient.GetBalanceAsync(accountId, cancellationToken);
            if (balance is null)
            {
                return new BalanceBlocked("INVALID", accountId, _blockedLogger);
            }

            if (balance.Blocked)
            {
                return new BalanceBlocked("BLOCKED", accountId, _blockedLogger);
            }

            return new BalanceCalculator(_balanceClient, accountId, balance.Amount, _calculatorLogger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for accountId={AccountId}", accountId);
            return new BalanceBlocked("ERROR", accountId, _blockedLogger);
        }
    }
}
