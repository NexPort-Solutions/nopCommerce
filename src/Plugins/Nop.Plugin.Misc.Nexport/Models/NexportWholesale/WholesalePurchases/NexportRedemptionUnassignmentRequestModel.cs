using System;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportRedemptionUnassignmentRequestModel : BaseNopEntityModel
    {
        public Guid InvoiceItemId { get; set; }

        public int RequestedByCustomerId { get; set; }

        public string CustomerComments { get; set; }

        public string StaffNotes { get; set; }

        public NexportRedemptionUnassignmentRequestStatus RequestStatus { get; set; }

        public DateTime UtcCreatedDate { get; set; }

        public DateTime? UtcLastModifiedDate { get; set; }

        public string ReasonForUnassignment { get; set; }
    }
}
