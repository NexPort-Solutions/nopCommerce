using System;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public class PendingOrderCancellationRequestModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.Customer")]
        public int CustomerId { get; set; }

        public string CustomerInfo { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.CustomerComments")]
        public string CustomerComments { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.ReasonForCancellation")]
        public string ReasonForCancellation { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.RequestStatus")]
        public PendingOrderCancellationRequestStatus RequestStatus { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.StaffNotes")]
        public string StaffNotes { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.UtcCreatedDate")]
        public DateTime UtcCreatedDate { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.Fields.UtcLastModifiedDate")]
        public DateTime? UtcLastModifiedDate { get; set; }
    }
}