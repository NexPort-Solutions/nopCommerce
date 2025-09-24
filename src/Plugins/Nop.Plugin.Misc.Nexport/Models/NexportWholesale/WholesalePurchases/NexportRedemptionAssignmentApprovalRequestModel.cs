using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static NexportApi.Model.RedeemInvoiceItemRequest;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionAssignmentApprovalRequestModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.Product")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.Product")]
    public Product Product { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedeemingProduct")]
    public int? RedeemingProductId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedeemingProduct")]
    [CanBeNull]
    public Product RedeemingProduct { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedemptionAssignmentType")]
    public NexportRedemptionAssignmentTypeStatus RedemptionAssignmentType { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedemptionEmail")]
    public string RedemptionEmail { get; set; }

    public string RedemptionFirstName { get; set; }

    public string RedemptionLastName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.InvoiceItemId")]
    public Guid InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedeemingCustomer")]
    public Guid? RedemptionUserId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedeemingCustomer")]
    public int? RedemptionCustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RedeemingCustomer")]
    public string RedemptionCustomerInfo { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.UtcRedemptionStartDate")]
    public DateTime? UtcRedemptionStartDate { get; set; }

    public int? StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.Store")]
    public string Store { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.PurchasingGroup")]
    public Guid? PurchasingGroupId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.PurchasingGroup")]
    public string PurchasingGroup { get; set; }

    public bool IsOpenEnded { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.ExtensionOption")]
    public RedemptionActionTypeEnum? ExtensionOption { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.ExtensionOption")]
    public IList<SelectListItem> AvailableExtensionOptions { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.UtcCreatedDate")]
    public DateTime UtcCreatedDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.UtcModifiedDate")]
    public DateTime? UtcModifiedDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RequestStatus")]
    public NexportRedemptionAssignmentApprovalRequestStatus Status { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.Notes")]
    public string Notes { get; set; }

    public int RequestedByCustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.RequestedByCustomer")]
    public string RequestedByCustomerInfo { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.ApprovedBy")]
    public int? ApprovedByCustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionAssignmentApprovalRequests.Fields.ApprovedBy")]
    public string ApprovedByCustomerInfo { get; set; }
}