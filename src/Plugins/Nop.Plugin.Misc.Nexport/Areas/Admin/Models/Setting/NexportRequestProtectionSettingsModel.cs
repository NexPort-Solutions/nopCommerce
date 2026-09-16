using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Setting;

public record NexportRequestProtectionSettingsModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.RequestProtection.BlockRecognizedCrawlersOnAuthenticationPages")]
    public bool BlockRecognizedCrawlersOnAuthenticationPages { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.RequestProtection.BlockKnownProbePaths")]
    public bool BlockKnownProbePaths { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.RequestProtection.BlockedRequestFileExtensions")]
    public string BlockedRequestFileExtensions { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.RequestProtection.BlockedRequestPathPrefixes")]
    public string BlockedRequestPathPrefixes { get; set; } = string.Empty;
}