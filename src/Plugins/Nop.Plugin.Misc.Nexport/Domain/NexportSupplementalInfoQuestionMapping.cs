using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoQuestionMapping : BaseEntity
    {
        public int QuestionId { get; set; }

        public int ProductMappingId { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
