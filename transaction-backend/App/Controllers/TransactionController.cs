using Microsoft.AspNetCore.Mvc;
using Transactions.Backend.Domain.Models;
using Transactions.Backend.App.Services;

namespace Transactions.Backend.App.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionController(TransactionService transactionService) : ControllerBase
{
    [HttpGet("{accountId}/{transactionId}")]
    public async Task<ActionResult<Transaction>> GetTransactionAsync(string accountId, int transactionId, CancellationToken cancellationToken)
    {
        var transaction = await transactionService.ReadAsync(accountId, transactionId, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransactionAsync([FromBody] Transaction transaction, CancellationToken cancellationToken)
    {
        if (transaction?.Account == null)
            return BadRequest();
        
        var updated = await transactionService.CreateAsync(transaction.Account.ToAccountId(), (int)transaction.TransactionId!, (decimal)transaction.Amount!, transaction.Status!, cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
