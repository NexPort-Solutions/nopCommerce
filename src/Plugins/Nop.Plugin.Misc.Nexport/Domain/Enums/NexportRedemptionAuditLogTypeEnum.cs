using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportRedemptionAuditLogTypeEnum
{
    [Display(Name = "Assign Redemption")]
    AssignRedemption = 1,

    [Display(Name = "Unassign Redemption")]
    UnassignRedemption = 2,

    [Display(Name = "Refund Redemption")]
    RefundRedemption = 3,

    [Display(Name = "Redemption Redeemed")]
    RedemptionRedeemed = 4,

    [Display(Name = "Unassign Request")]
    RequestUnassign = 5,

    [Display(Name = "Refund Request")]
    RequestRefund = 6,

    [Display(Name = "Cancel Awaiting")]
    CancelAwaiting = 7,

    [Display(Name = "Other")]
    Other = 100
}