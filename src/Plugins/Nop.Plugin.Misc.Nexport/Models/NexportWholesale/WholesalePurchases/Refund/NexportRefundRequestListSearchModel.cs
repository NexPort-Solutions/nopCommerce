using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.Refund;

public record NexportRefundRequestListSearchModel : BaseSearchModel
{
    public NexportRefundRequestListSearchModel()
    {
        SetGridPageSize();

        AvailableStores = new List<SelectListItem>();
    }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
}
