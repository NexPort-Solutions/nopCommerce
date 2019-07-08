using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportUserMapping : BaseEntity
    {
        public int NopUserId { get; set; }

        public Guid NexportUserId { get; set; }

        public DateTime? UtcDateSynchronize { get; set; }
    }
}
