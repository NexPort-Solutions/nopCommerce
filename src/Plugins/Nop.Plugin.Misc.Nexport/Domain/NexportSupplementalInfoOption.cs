using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoOption : BaseEntity
    {
        public int QuestionId { get; set; }

        public string OptionText { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }

        public bool Deleted { get; set; }
    }
}
