using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportGroupProductRedemptionModel : BaseNopEntityModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProductName { get; set; }
        public string Status { get; set; } = "Available";
        public string? DateRedeemed { get; set; }
        public Guid InvoiceItemId { get; set; }
        public string? PurchasedBy { get; set; }
        public string? PurchasedIn { get; set; }

    }
}
