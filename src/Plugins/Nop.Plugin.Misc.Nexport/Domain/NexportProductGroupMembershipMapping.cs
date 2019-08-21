using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportProductGroupMembershipMapping : BaseEntity
    {
        public int NexportProductMappingId { get; set; }

        public Guid NexportGroupId { get; set; }

        public string NexportGroupName { get; set; }

        public string NexportGroupShortName { get; set; }
    }
}
