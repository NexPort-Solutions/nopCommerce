using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;

public record NexportReturnRequestModel : ReturnRequestModel
{
    public string NexportUserProfileLink { get; set; }

    public bool IsNexportPurchase { get; set; }

    public bool IsNexportWholesalePurchase { get; set; }

    public Guid? InvoiceItemId { get; set; }

    public int RefundOption { get; set; }
}