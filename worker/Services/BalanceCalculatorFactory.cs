using Transactions.Worker.Repositories;

namespace Transactions.Worker.Services;

public sealed class BalanceCalculatorFactory
{
    private readonly BalanceRepository _balanceRepository;
    private readonly ILogger<BalanceCalculatorFactory> _logger;
    private readonly ILogger<BalanceBlocked> _blockedLogger;
    private readonly ILogger<BalanceCalculator> _calculatorLogger;

    public BalanceCalculatorFactory(
        BalanceRepository balanceRepository,
        ILogger<BalanceCalculatorFactory> logger,
        ILogger<BalanceBlocked> blockedLogger,
        ILogger<BalanceCalculator> calculatorLogger)
    {
        _balanceRepository = balanceRepository;
        _logger = logger;
        _blockedLogger = blockedLogger;
        _calculatorLogger = calculatorLogger;
    }

    public async Task<IBalanceCalculator> CreateAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _balanceRepository.GetBalanceAsync(accountId, cancellationToken);
            if (balance is null)
            {
                return new BalanceBlocked("ERROR", accountId, _blockedLogger);
            }

            if (balance.Blocked)
            {
                return new BalanceBlocked("BLOCKED", accountId, _blockedLogger);
            }

            return new BalanceCalculator(_balanceRepository, accountId, balance.Amount, _calculatorLogger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for accountId={AccountId}", accountId);
            return new BalanceBlocked("ERROR", accountId, _blockedLogger);
        }
    }
}
