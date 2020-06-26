using System;
using Nop.Core;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Domains
{
    public class PendingOrderCancellationRequest : BaseEntity
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public string CustomerComments { get; set; }

        public string ReasonForCancellation { get; set; }

        public string StaffNotes { get; set; }

        public PendingOrderCancellationRequestStatus RequestStatus { get; set; }

        public DateTime UtcCreatedDate { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }
    }
}
