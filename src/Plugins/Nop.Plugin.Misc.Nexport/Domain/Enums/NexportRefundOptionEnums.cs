using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportRefundOptionEnums
{
    [Display(Name = "Expiring current enrollment")]
    ExpireEnrollment = 1,

    [Display(Name = "Drop current enrollment")]
    DropEnrollment = 2,

    [Display(Name = "Destroy current enrollment")]
    DestroyEnrollment = 3
}