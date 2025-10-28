using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Setting;

public record NexportStoreSettingsModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.Store.RedirectAfterOrderConfirmation")]
    public bool RedirectAfterOrderConfirmation { get; set; }
    public bool RedirectAfterOrderConfirmation_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.Store.RedirectAfterOrderConfirmationPath")]
    public string RedirectAfterOrderConfirmationPath { get; set; }
    public bool RedirectAfterOrderConfirmationPath_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Configuration.Settings.Store.DisplayManagePurchasesLink")]
    public bool DisplayManagePurchasesLink { get; set; }
    public bool DisplayManagePurchasesLink_OverrideForStore { get; set; }
}