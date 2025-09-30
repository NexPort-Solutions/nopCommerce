using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;

public record ConfirmStepModel : RedeemStepModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.FirstName")]
    public string FirstName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.LastName")]
    public string LastName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.Product")]
    public string Product { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.RedeemingProduct")]
    public string RedeemingProduct { get; set; }

    public int PurchasingProductMappingId { get; set; }

    public int? RedeemingProductMappingId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Confirm.RedemptionMethod")]
    public string SendViaEmail { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.StartDate")]
    public DateTime? UtcStartDate { get; set; }

    public int? StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.Store")]
    public string StoreName { get; set; }

    public Guid? PurchasingGroupId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.PurchasingGroup")]
    public string PurchasingGroupName { get; set; }

    public List<NexportProductGroupMembershipMappingModel> GroupMembershipMappings { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.ExtensionOption")]
    public RedeemInvoiceItemRequest.RedemptionActionTypeEnum? ExtensionOption { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Option.RequireApproval")]
    public bool RequireApproval { get; set; }
}