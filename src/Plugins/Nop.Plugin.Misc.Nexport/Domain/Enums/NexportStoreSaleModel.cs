using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums
{
    public enum NexportStoreSaleModel
    {
        [Display(Name = "Retail")]
        Retail = 0,

        [Display(Name = "Wholesale")]
        Wholesale = 1
    }
}
