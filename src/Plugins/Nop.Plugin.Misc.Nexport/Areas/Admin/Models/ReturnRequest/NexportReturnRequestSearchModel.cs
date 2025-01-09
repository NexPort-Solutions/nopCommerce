using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;

public record NexportReturnRequestSearchModel : ReturnRequestSearchModel
{
    //[NopResourceDisplayName("Plugins.Misc.Category.HasProductMapping")]
    public bool IsNexportPurchase { get; set; } = false;

    //[NopResourceDisplayName("Plugins.Misc.Category.HasProductMapping")]
    public bool IsNexportWholesalePurchase { get; set; } = false;
}