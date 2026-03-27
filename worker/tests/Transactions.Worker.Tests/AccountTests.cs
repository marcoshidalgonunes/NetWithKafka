using Transactions.Worker.Models;
using Xunit;

namespace Transactions.Worker.Tests;

public sealed class AccountTests
{
    [Fact]
    public void ToAccountId_ConcatenatesBranchAndNumber()
    {
        var account = new Account { Branch = "1234", Number = "567890" };

        var id = account.ToAccountId();

        Assert.Equal("1234567890", id);
    }
}
