using System;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportRedemptionUnassignmentRequestModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.InvoiceItemId")]
        public Guid InvoiceItemId { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.Customer")]
        public int RequestedByCustomerId { get; set; }

        public string CustomerInfo { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.CustomerComments")]
        public string CustomerComments { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.StaffNotes")]
        public string StaffNotes { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.RequestStatus")]
        public NexportRedemptionUnassignmentRequestStatus RequestStatus { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.UtcCreatedDate")]
        public DateTime UtcCreatedDate { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        [NopResourceDisplayName("RedemptionUnassignmentRequests.Fields.ReasonForUnassignment")]
        public string ReasonForUnassignment { get; set; }
    }
}







