using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;

public record NexportReturnRequestModel : ReturnRequestModel
{
    public string RequestUserNexportProfileLink { get; set; }

    public int? AssignedUserCustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.InvoiceItemRefund.AssignedUser")]
    public Guid? AssignedUserNexportId { get; set; }

    public string AssignedUserInfo { get; set; }

    public string AssignedUserNexportProfileLink { get; set; }

    public bool IsNexportPurchase { get; set; }

    public bool IsNexportWholesalePurchase { get; set; }

    public Guid? InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.InvoiceItemRefund.Option")]
    public int RefundOption { get; set; }
}