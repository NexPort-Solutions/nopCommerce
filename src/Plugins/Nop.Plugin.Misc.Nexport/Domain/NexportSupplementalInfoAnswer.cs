using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoAnswer : BaseEntity
    {
        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public int QuestionId { get; set; }

        public int OptionId { get; set; }

        public NexportSupplementalInfoAnswerStatus Status { get; set; }

        public DateTime? UtcDateProcessed { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
