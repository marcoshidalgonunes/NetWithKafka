namespace Transactions.Bff.Models;

public sealed class Controls
{
    public string? OperationType { get; set; }
    public bool? IsRealOperation { get; set; }
    public string? ValidationLevel { get; set; }
    public string? TransactionOrigin { get; set; }
    public string? ObservationType { get; set; }
    public bool? IsPrincipal { get; set; } = true;
    public bool? IsDebitToAvailable { get; set; }
    public bool? ShouldUseLimit { get; set; } = true;
    public string? TimeDelay { get; set; } = "";
    public bool? IsEscrowAccount { get; set; } = false;
    public string? ProvisionalType { get; set; } = "";
}
