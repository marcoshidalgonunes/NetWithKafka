using Microsoft.AspNetCore.Mvc;
using Transactions.Bff.Models;
using Transactions.Bff.Services;

namespace Transactions.Bff.Controllers;

[ApiController]
[Route("api")]
public sealed class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("process")]
    public async Task<ActionResult<Transaction>> Process([FromBody] Transaction payload, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _transactionService.SendAndReceiveAsync(payload, cancellationToken);
            if (result is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }
        catch (TimeoutException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
    }
}
