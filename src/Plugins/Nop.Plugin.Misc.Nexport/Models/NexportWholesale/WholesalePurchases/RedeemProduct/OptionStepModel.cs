using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;

public record OptionStepModel : RedeemStepModel
{
    public Guid? RedeemingUserId { get; set; }

    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.StartDate")]
    public DateTime? UtcStartDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.Store")]
    public int? StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.PurchasingGroup")]
    public Guid? PurchasingGroupId { get; set; }

    public int? ExtensionAction { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableNexportPurchasingGroups { get; set; } = new List<SelectListItem>();

    public bool IsOpenEnded { get; set; }

    public bool RequireApproval { get; set; }
}