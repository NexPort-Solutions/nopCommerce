using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportOrderInvoiceItemRedemptionStatus
{
    [Display(Name = "Processing Available")]
    ProcessingAvailable = 0,

    [Display(Name = "Available")]
    Available = 1,

    [Display(Name = "Awaiting")]
    Awaiting = 2,

    [Display(Name = "Assigned")]
    Assigned = 3,

    [Display(Name = "Processing Awaiting")]
    ProcessingAwaiting = 4,

    [Display(Name = "Refunded")]
    Refunded = 5,

    [Display(Name = "Processing Refund")]
    ProcessingRefund = 6,

    [Display(Name = "Approval Awaiting")]
    ApprovalAwaiting = 7
}