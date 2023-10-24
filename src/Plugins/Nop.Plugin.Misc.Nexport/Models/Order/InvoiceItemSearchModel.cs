using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public record InvoiceItemSearchModel : BaseSearchModel
{
    public InvoiceItemSearchModel() => SetGridPageSize();

    public int Id { get; set; }
}
