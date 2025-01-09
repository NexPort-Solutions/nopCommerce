using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportPurchasesByFundingPoolListSearchModel : BaseSearchModel
{
    //[NopResourceDisplayName("Plugins.Misc.Nexport.Group.SearchName")]
    public string SearchFundingPoolName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchStatus")]
    public NexportOrderInvoiceItemRedemptionStatus? SearchStatusId { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }

    public NexportPurchasesByFundingPoolListSearchModel()
    {
        SetGridPageSize();

        AvailableStatuses = new List<SelectListItem>();
    }
}