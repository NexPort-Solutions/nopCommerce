namespace Nop.Plugin.Misc.Nexport.Configuration;

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

    public int RequestsPerWindow { get; protected set; }

    public int MaxConcurrentRequests { get; protected set; }
}

public partial class NexportRequestProtectionConfig
{
    public bool Enabled { get; protected set; }

    public int RateLimitWindowSeconds { get; protected set; } = 60;

    public NexportEndpointProtectionConfig LoginPageEndpoint { get; protected set; } = new(600, 30);

    public NexportEndpointProtectionConfig LoginSubmissionEndpoint { get; protected set; } = new(120, 12);

    public NexportEndpointProtectionConfig RegistrationPageEndpoint { get; protected set; } = new(240, 15);

    public NexportEndpointProtectionConfig RegistrationSubmissionEndpoint { get; protected set; } = new(30, 5);
}
