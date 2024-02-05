
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct
{
    public record EmailInfoStepModel : BaseNopEntityModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public int? CurrentProductId { get; set; }
        public Guid InvoiceItemId { get; set; }
    }
}
