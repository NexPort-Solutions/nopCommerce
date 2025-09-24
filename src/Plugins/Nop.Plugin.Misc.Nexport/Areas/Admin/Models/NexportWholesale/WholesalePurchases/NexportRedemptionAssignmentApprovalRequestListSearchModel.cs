using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionAssignmentApprovalRequestListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.SearchStartDate")]
    [UIHint("DateNullable")]
    public DateTime? StartDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.SearchEndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedemptionUnassignmentRequests.RequestStatus")]
    public int RequestStatusId { get; set; }

    public IList<SelectListItem> RequestStatusList { get; set; } = new List<SelectListItem>();

    public NexportRedemptionAssignmentApprovalRequestListSearchModel()
    {
        SetGridPageSize();
    }
}