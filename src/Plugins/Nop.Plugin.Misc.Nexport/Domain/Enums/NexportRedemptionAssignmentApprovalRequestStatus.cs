using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportRedemptionAssignmentApprovalRequestStatus
{
    [Display(Name = "Received")]
    Received = 10,

    [Display(Name = "Accepted")]
    Accepted = 20,

    [Display(Name = "Rejected")]
    Rejected = 30
}