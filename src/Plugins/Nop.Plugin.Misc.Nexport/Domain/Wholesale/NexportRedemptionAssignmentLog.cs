using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

public class NexportRedemptionAssignmentLog : BaseEntity
{
    public Guid InvoiceItemId { get; set; }

    public string Description { get; set; }

    public int CustomerId { get; set; }

    public NexportRedemptionAssignmentLogTypeEnum Type { get; set; }

    public DateTime UtcCreatedDate { get; set; }
}