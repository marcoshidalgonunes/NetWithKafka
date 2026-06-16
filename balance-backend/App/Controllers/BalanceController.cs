using Microsoft.AspNetCore.Mvc;
using Balances.Backend.Domain.Models;
using Balances.Backend.App.Services;

namespace Balances.Backend.App.Controllers;

[ApiController]
[Route("api/balance")]
public sealed class BalanceController(BalanceService balanceService) : ControllerBase
{
    [HttpGet("{accountId}")]
    public async Task<ActionResult<Balance>> GetBalanceAsync(string accountId, CancellationToken cancellationToken)
    {
        var balance = await balanceService.ReadAsync(accountId, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpPut("{accountId}")]
    public async Task<IActionResult> UpdateBalanceAsync(string accountId, [FromBody] decimal newAmount, CancellationToken cancellationToken)
    {
        var updated = await balanceService.UpdateAsync(accountId, newAmount, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
