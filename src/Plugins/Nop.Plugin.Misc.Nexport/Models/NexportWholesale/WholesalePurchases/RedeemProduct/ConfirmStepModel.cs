using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;

public record ConfirmStepModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.FirstName")]
    public string FirstName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.LastName")]
    public string LastName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.Product")]
    public string Product { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.RedemptionMethod")]
    public string SendViaEmail { get; set; }
}