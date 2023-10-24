using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class OrderInvoiceRedemptionQueueItem : BaseEntity
{
    public required int OrderInvoiceItemId { get; set; }
    public required Guid RedeemingUserId { get; set; }
    public required int ProductMappingId { get; set; }
    public required int OrderItemId { get; set; }
    public required DateTime UtcDateCreated { get; set; }
    public DateTime? UtcLastFailedDate { get; set; }
    public required int RetryCount { get; set; }
    public int? ManualApprovalAction { get; set; }
}
