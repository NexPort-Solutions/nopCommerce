using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums
{
    public enum NexportOrderInvoiceItemRedemptionStatus
    {
        [Display(Name = "Processing")]
        ProcessingAvailable = 0,

        [Display(Name = "Available")]
        Available = 1,

        [Display(Name = "Awaiting")]
        Awaiting = 2,

        [Display(Name = "Assigned")]
        Assigned = 3,

        [Display(Name = "Processing")]
        ProcessingAwaiting = 4
    }
}
