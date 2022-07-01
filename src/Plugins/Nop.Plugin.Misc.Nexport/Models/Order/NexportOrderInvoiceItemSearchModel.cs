using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public record NexportOrderInvoiceItemSearchModel : BaseSearchModel
    {
        public NexportOrderInvoiceItemSearchModel()
        {
            SetGridPageSize();
        }

        public int OrderId { get; set; }
    }
}
