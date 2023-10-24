using System.ComponentModel.DataAnnotations;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class Answer : BaseEntity
{
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
    public AnswerStatus Status { get; set; }
    public DateTime? UtcDateProcessed { get; set; }
    public DateTime UtcDateCreated { get; set; }
    public DateTime? UtcDateModified { get; set; }

    public enum AnswerStatus
    {
        [Display(Name = "Not processed")]
        NotProcessed = 0,

        [Display(Name = "Processed")]
        Processed = 1,

        [Display(Name = "Modified")]
        Modified = 2,
    }
}
