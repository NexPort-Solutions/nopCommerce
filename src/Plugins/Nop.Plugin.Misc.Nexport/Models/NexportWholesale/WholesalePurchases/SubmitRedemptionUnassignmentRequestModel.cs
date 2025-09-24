using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record SubmitRedemptionUnassignmentRequestModel : BaseNopModel
{
    public Guid InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.UnassignReason")]
    public int RedemptionUnassignmentRequestReasonId { get; set; }

    public IList<NexportRedemptionUnassignmentRequestReasonModel> AvailableUnassignmentReasons { get; set; } = new List<NexportRedemptionUnassignmentRequestReasonModel>();

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.Comments")]
    public string Comments { get; set; }

    public string Result { get; set; }

    public string ProductName { get; set; }

    public string GroupName { get; set; }

    public string CustomerName { get; set; }
}