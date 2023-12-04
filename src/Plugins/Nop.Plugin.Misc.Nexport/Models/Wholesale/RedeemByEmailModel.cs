
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record RedeemByEmailModel : BaseNopEntityModel
    {
        public string? ProductName { get; set; }
        public int InvoiceItemId { get; set; }
        public DateTime RedeemedDate { get; set; }
        public bool Redeemed { get; set; }
    }
}
