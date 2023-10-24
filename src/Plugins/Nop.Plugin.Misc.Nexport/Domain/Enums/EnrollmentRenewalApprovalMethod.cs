using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum EnrollmentRenewalApprovalMethod
{
    [Display(Name = "Auto")]
    Auto = 0,

    [Display(Name = "Manual")]
    Manual = 1,
}
