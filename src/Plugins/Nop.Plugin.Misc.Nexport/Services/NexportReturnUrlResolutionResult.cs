namespace Nop.Plugin.Misc.Nexport.Services;

public enum NexportReturnUrlResolutionStatus
{
    Missing,
    Valid,
    Canonicalized,
    Rejected
}

public sealed class NexportReturnUrlResolutionResult
{
    public NexportReturnUrlResolutionResult(NexportReturnUrlResolutionStatus status, string destination = null)
    {
        Status = status;
        Destination = destination;
    }

    public NexportReturnUrlResolutionStatus Status { get; }

    public string Destination { get; }
}
