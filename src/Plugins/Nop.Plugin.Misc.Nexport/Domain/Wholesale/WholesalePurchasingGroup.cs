using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale
{
    public class WholesalePurchasingGroup : BaseEntity
    {
        public Guid NexportGroupId { get; set; }

        public string? NexportGroupName { get; set; }

        public string? NexportGroupShortName { get; set; }
    }
}