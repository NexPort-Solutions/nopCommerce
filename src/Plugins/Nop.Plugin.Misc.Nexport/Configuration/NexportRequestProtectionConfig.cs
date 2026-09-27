namespace Nop.Plugin.Misc.Nexport.Configuration;

public enum NexportStoreHostGuardMode
{
    // No host checks or background store refreshes.
    Disabled,
    // Record unknown hosts without blocking them; use to audit store URL configuration.
    Observe,
    // Reject unknown hosts once a valid store URL snapshot has loaded.
    Enforce
}

public partial class NexportEndpointProtectionConfig
{
    public NexportEndpointProtectionConfig()
    {
    }

    public NexportEndpointProtectionConfig(int requestsPerWindow, int maxConcurrentRequests)
    {
        RequestsPerWindow = requestsPerWindow;
        MaxConcurrentRequests = maxConcurrentRequests;
    }

    // Per-client-IP request budget for this policy on each app instance.
    public int RequestsPerWindow { get; protected set; }

    // Maximum active requests for this policy across all clients on each app instance.
    public int MaxConcurrentRequests { get; protected set; }
}

public partial class NexportRequestProtectionConfig
{
    // Master switch for plugin protection. Bound from deployment configuration at startup.
    public bool Enabled { get; protected set; }

    // Defaults to Disabled. The store URL is the only source of admitted authorities.
    public NexportStoreHostGuardMode StoreHostGuardMode { get; protected set; }

    public int RateLimitWindowSeconds { get; protected set; } = 60;

    public NexportEndpointProtectionConfig LoginPageEndpoint { get; protected set; } = new(600, 30);

    public NexportEndpointProtectionConfig LoginSubmissionEndpoint { get; protected set; } = new(120, 12);

    public NexportEndpointProtectionConfig RegistrationPageEndpoint { get; protected set; } = new(240, 15);

    public NexportEndpointProtectionConfig RegistrationSubmissionEndpoint { get; protected set; } = new(30, 5);
}