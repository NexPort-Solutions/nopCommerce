using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportRedemptionAssignmentTypeStatus
{
    [Display(Name = "Instant")]
    Instant = 1,

    [Display(Name = "Email")]
    Email = 2
}