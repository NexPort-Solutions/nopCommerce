using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class NexportStoreSettings : ISettings
{
    public bool RedirectAfterOrderConfirmation { get; set; }

    public string RedirectAfterOrderConfirmationPath { get; set; }

    public bool DisplayManagePurchasesLink { get; set; }
}