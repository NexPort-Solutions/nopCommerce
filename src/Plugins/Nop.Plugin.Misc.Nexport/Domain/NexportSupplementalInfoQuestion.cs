using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoQuestion : BaseEntity
    {
        public string QuestionText { get; set; }

        public string Description { get; set; }

        public NexportSupplementalInfoQuestionType Type { get; set; }

        public bool IsActive { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
