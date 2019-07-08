using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportOrderProcessingQueueItem : BaseEntity
    {
        public int OrderId { get; set; }

        public DateTime UtcDateCreated { get; set; }
    }
}
