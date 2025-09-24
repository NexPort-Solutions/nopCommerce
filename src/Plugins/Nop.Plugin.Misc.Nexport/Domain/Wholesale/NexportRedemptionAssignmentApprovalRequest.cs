using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Domain.Wholesale;

public class NexportRedemptionAssignmentApprovalRequest : BaseEntity
{
    public int ProductId { get; set; }

    public int? RedeemingProductId { get; set; }

    public NexportRedemptionAssignmentTypeStatus RedemptionAssignmentType { get; set; }

    public string RedemptionEmail { get; set; }

    public string RedemptionFirstName { get; set; }

    public string RedemptionLastName { get; set; }

    public Guid InvoiceItemId { get; set; }

    public Guid? RedemptionUserId { get; set; }

    public DateTime? UtcRedemptionStartDate { get; set; }

    public int? StoreId { get; set; }

    public Guid? PurchasingGroupId { get; set; }

    public bool IsOpenEnded { get; set; }

    public int? ExtensionOption { get; set; }

    public DateTime UtcCreatedDate { get; set; }

    public DateTime? UtcModifiedDate { get; set; }

    public NexportRedemptionAssignmentApprovalRequestStatus Status { get; set; }

    public string Notes { get; set; }

    public int RequestedByCustomerId { get; set; }

    public int? ApprovedByCustomerId { get; set; }
}