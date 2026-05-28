using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Transactions.Bff.App.Controllers;
using Transactions.Bff.Domain.Contracts;
using Transactions.Bff.Domain.Models;
using Xunit;

namespace Transactions.Bff.Tests;

public sealed class TransactionControllerTests
{
    [Fact]
    public async Task Process_Returns504_WhenTimeoutOccurs()
    {
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.SendAndReceiveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException());

        var controller = new TransactionController(service.Object);

        var result = await controller.Process(new Transaction(), CancellationToken.None);

        var statusResult = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, statusResult.StatusCode);
    }

    [Fact]
    public async Task Process_Returns200_WhenReplyArrives()
    {
        var reply = new Transaction { Status = "APPROVED" };
        var service = new Mock<ITransaction>();
        service
            .Setup(s => s.SendAndReceiveAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reply);

        var controller = new TransactionController(service.Object);

        var result = await controller.Process(new Transaction(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<Transaction>(ok.Value);
        Assert.Equal("APPROVED", payload.Status);
    }
}
