using System;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

public class NexportRedemptionAuditLog : BaseEntity
{
    public Guid InvoiceItemId { get; set; }

    public string Description { get; set; }

    public int CustomerId { get; set; }

    public int? TargetedCustomerId { get; set; }

    public NexportRedemptionAuditLogTypeEnum Type { get; set; }

    public DateTime UtcDateCreated { get; set; }
}