using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
public record RedeemActionModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.ExtensionOption")]
    public int? ExtensionAction { get; set; }

    public IList<SelectListItem> AvailableExtensionActions { get; set; } = new List<SelectListItem>();

    public bool HasExistingEnrollment { get; set; }

    public bool RequireApproval { get; set; }
}