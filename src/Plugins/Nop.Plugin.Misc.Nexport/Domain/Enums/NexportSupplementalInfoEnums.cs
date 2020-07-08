using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums
{
    public enum NexportSupplementalInfoQuestionType
    {
        [Display(Name = "Single option")]
        SingleOption = 0,

        [Display(Name = "Multiple options")]
        MultipleOptions = 1
    }

    public enum NexportSupplementalInfoAnswerStatus
    {
        [Display(Name = "Not processed")]
        NotProcessed = 0,

        [Display(Name = "Processed")]
        Processed = 1,

        [Display(Name = "Modified")]
        Modified = 2,
    }
}