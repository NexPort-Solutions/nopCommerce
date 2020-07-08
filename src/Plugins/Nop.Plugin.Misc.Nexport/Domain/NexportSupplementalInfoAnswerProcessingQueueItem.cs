using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoAnswerProcessingQueueItem : BaseEntity
    {
        public int AnswerId { get; set; }

        public DateTime UtcDateCreated { get; set; }
    }
}
