using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain;

public class OrderInvoiceItem : BaseEntity
{
    public required int OrderId { get; set; }
    public required int OrderItemId { get; set; }
    public required Guid InvoiceItemId { get; set; }
    public required Guid InvoiceId { get; set; }
    public required DateTime UtcDateProcessed { get; set; }
    public Guid? RedeemingUserId { get; set; }
    public Guid? RedemptionEnrollmentId { get; set; }
    public DateTime? UtcDateRedemption { get; set; }
    public bool? RequireManualApproval { get; set; }
    public string? InvoiceRedemptionCode { get; set; }
    public string? InvoiceItemRedemptionCode { get; set; }
}
