using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class NexportRequestProtectionSettings : ISettings
{
    public bool BlockRecognizedCrawlersOnAuthenticationPages { get; set; }

    public bool BlockKnownProbePaths { get; set; }

    public List<string> BlockedRequestFileExtensions { get; set; } = new()
    {
        ".php"
    };

    public List<string> BlockedRequestPathPrefixes { get; set; } = new()
    {
        "/wp-admin",
        "/wp-content",
        "/wp-includes",
        "/wp-json",
        "/vendor/phpunit",
        "/cgi-bin",
        "/actuator"
    };
}