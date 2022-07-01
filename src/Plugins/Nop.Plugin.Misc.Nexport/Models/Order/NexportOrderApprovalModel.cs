using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public record NexportOrderApprovalModel : BaseNopModel
    {
        public NexportOrderInvoiceItemSearchModel SearchModel { get; set; }
    }
}
