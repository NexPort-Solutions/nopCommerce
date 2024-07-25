using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

public class NexportRedemptionUnassignmentRequest : BaseEntity
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