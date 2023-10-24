using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

public class AnswerProcessingQueueItem : BaseEntity
{
    public int AnswerId { get; set; }
    public DateTime UtcDateCreated { get; set; }
}
