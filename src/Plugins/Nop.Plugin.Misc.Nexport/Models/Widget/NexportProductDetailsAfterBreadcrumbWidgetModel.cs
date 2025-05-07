using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Widget;

public record NexportProductDetailsAfterBreadcrumbWidgetModel : BaseNopModel
{
    public int TotalPurchases { get; set; }

    public int? LastPurchaseOrderId { get; set; }

    public DateTime? LastPurchaseDate { get; set; }
}
