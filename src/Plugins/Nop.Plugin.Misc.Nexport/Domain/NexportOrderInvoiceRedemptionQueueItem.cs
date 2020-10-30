using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain
{
    public class NexportOrderInvoiceRedemptionQueueItem : BaseEntity
    {
        public int OrderInvoiceItemId { get; set; }

        public Guid RedeemingUserId { get; set; }

        public int ProductMappingId { get; set; }

        public int OrderItemId { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime? UtcLastFailedDate { get; set; }

        public int RetryCount { get; set; }

        public int? ManualApprovalAction { get; set; }
    }
}
