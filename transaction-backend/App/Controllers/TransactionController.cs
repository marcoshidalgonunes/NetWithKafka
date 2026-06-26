using Microsoft.AspNetCore.Mvc;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.App.Services;

namespace Transactions.Backend.App.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionController(TransactionService transactionService) : ControllerBase
{
    [HttpGet("{accountId}/{date}")]
    public async Task<ActionResult<Transaction>> GetTransactionsByDateAsync(string accountId, string date, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(date, out var dateOnly))
        {
            return BadRequest("Invalid date format.");
        }

        var transaction = await transactionService.ReadByDateAsync(accountId, dateOnly, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransactionAsync([FromBody] Transaction transaction, CancellationToken cancellationToken)
    {
        if (transaction?.Entry.Account == null)
            return BadRequest();
        
        var updated = await transactionService.CreateAsync(transaction, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
