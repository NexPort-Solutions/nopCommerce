using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportSupplementalInfoOptionGroupAssociation : BaseEntity
    {
        public int OptionId { get; set; }

        public Guid NexportGroupId { get; set; }

        public string NexportGroupName { get; set; }

        public string NexportGroupShortName { get; set; }

        public bool IsActive { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcDateModified { get; set; }
    }
}
