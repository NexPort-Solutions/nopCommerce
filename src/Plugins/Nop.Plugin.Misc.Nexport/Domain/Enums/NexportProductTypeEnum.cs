using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums
{
    public enum NexportProductTypeEnum
    {
        [Display(Name = "Catalog")]
        Catalog = 0,

        [Display(Name = "Section")]
        Section = 1,

        [Display(Name = "Training Plan")]
        TrainingPlan = 2
    }
}
