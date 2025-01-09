using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;

public record NexportReturnRequestModel : ReturnRequestModel
{
    public bool IsNexportPurchase { get; set; }

    public bool IsNexportWholesalePurchase { get; set; }
}