using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportOrderInvoiceItem : BaseEntity
    {
        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

        public Guid InvoiceItemId { get; set; }

        public Guid InvoiceId { get; set; }

        public DateTime UtcDateProcessed { get; set; }

        public Guid? RedeemingUserId { get; set; }

        public Guid? RedemptionEnrollmentId { get; set; }

        public DateTime? UtcDateRedemption { get; set; }
    }
}
