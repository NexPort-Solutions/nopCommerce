using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Redemption;

public class RedemptionCreateModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Redemption.OrderInvoiceItemId")]
    public int OrderInvoiceItemId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Admin.Redemption.NopCustomerId")]
    public int NopCustomerId { get; init; }
}
