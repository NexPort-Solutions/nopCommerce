using System.ComponentModel.DataAnnotations;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class Question : BaseEntity
{
    public string? QuestionText { get; set; }
    public string? Description { get; set; }
    public QuestionType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime UtcDateCreated { get; set; }
    public DateTime? UtcDateModified { get; set; }

    public enum QuestionType
    {
        [Display(Name = "Single option")]
        SingleOption = 0,

        [Display(Name = "Multiple options")]
        MultipleOptions = 1,
    }
}
