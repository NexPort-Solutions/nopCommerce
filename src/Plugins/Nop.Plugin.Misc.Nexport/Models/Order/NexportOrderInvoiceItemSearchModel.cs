using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public class NexportOrderInvoiceItemSearchModel : BaseSearchModel
    {
        public NexportOrderInvoiceItemSearchModel()
        {
            SetGridPageSize();
        }

        public int OrderId { get; set; }
    }
}
