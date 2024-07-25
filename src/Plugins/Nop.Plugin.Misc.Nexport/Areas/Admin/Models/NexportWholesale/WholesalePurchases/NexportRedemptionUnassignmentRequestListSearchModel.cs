using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionUnassignmentRequestListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("RedemptionUnassignmentRequests.SearchStartDate")]
    [UIHint("DateNullable")]
    public DateTime? StartDate { get; set; }

    [NopResourceDisplayName("RedemptionUnassignmentRequests.SearchEndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [NopResourceDisplayName("RedemptionUnassignmentRequests.RequestStatus")]
    public int RequestStatusId { get; set; }

    public IList<SelectListItem> RequestStatusList { get; set; } = new List<SelectListItem>();

    public NexportRedemptionUnassignmentRequestListSearchModel()
    {
        SetGridPageSize();
    }
}