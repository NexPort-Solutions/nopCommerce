using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionUnassignmentRequestModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.InvoiceItemId")]
    public Guid InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.OrderId")]
    public int OrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.Customer")]
    public int RequestedByCustomerId { get; set; }

    public string CustomerInfo { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.CustomerComments")]
    public string CustomerComments { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.StaffNotes")]
    public string StaffNotes { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.RequestStatus")]
    public NexportRedemptionUnassignmentRequestStatus RequestStatus { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.UtcCreatedDate")]
    public DateTime UtcCreatedDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.UtcLastModifiedDate")]
    public DateTime? UtcLastModifiedDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Fields.ReasonForUnassignment")]
    public string ReasonForUnassignment { get; set; }
}