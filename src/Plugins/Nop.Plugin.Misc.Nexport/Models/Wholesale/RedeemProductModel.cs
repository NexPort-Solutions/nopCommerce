
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record RedeemProductModel : BaseNopEntityModel
    {
        public bool AdminView { get; set; } = false;

        public NexportGroupModel CurrentGroup { get; set; }

        public Product CurrentProduct { get; set; }

        public Guid InvoiceItemId { get; set; }
    }
}
