using Microsoft.AspNetCore.Mvc;
using Transactions.Bff.Domain.Contracts;
using Transactions.Bff.Domain.Models;

namespace Transactions.Bff.App.Controllers;

[ApiController]
[Route("api")]
public sealed class TransactionController(ITransaction transactionMessaging) : ControllerBase
{
    private readonly ITransaction _transactionMessaging = transactionMessaging;

    [HttpPost("process")]
    public async Task<ActionResult<Transaction>> Process([FromBody] Entry payload, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _transactionMessaging.SendAndReceiveAsync(payload, cancellationToken);
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
