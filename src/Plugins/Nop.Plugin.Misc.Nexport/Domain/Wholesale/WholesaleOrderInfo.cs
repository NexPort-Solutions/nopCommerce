using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale
{
    public class WholesaleOrderInfo : BaseEntity
    {
        public Guid? NexportGroupId { get; set; }

        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

        public int ProductId { get; set; }

        public int Available { get; set; }

        public int Awaiting { get; set; }

        public int Redeemed { get; set; }

    }
}