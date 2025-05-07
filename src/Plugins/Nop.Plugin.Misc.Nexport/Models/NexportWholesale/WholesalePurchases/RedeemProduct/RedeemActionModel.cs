using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
public record RedeemActionModel
{
    public int? ExtensionAction { get; set; }

    public IList<SelectListItem> AvailableExtensionActions { get; set; } = new List<SelectListItem>();

    public bool HasExistingEnrollment { get; set; }
}
