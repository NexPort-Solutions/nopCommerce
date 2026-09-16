using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

internal sealed class NexportRequestProtectionPolicyProvider
{
    private NexportRequestProtectionPolicySnapshot _current = NexportRequestProtectionPolicySnapshot.Disabled;

    public NexportRequestProtectionPolicySnapshot Current => Volatile.Read(ref _current);

    public void Update(NexportRequestProtectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var snapshot = new NexportRequestProtectionPolicySnapshot(
            settings.BlockRecognizedCrawlersOnAuthenticationPages,
            settings.BlockKnownProbePaths,
            new NexportRequestPathPolicy(
                settings.BlockedRequestPathPrefixes ?? new List<string>(),
                settings.BlockedRequestFileExtensions ?? new List<string>()));

        Volatile.Write(ref _current, snapshot);
    }
}

internal sealed record NexportRequestProtectionPolicySnapshot(
    bool BlockRecognizedCrawlersOnAuthenticationPages,
    bool BlockKnownProbePaths,
    NexportRequestPathPolicy RequestPathPolicy)
{
    public static NexportRequestProtectionPolicySnapshot Disabled { get; } =
        new(false, false, new NexportRequestPathPolicy(Array.Empty<string>(), Array.Empty<string>()));
}