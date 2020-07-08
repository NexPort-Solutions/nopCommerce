using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportGroupMembershipRemovalQueueItem : BaseEntity
    {
        public int CustomerId { get; set; }

        public Guid NexportMembershipId { get; set; }

        public DateTime UtcDateCreated { get; set; }
    }
}
