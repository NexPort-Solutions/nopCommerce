using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductRedemptionModel : BaseNopEntityModel
    {
        public string? Name { get; set; }
        public string? ProductName { get; set; }
        public string Status { get; set; } = "Available";
        public string? DateRedeemed { get; set; }
        public Guid InvoiceItemId { get; set; }

    }
}
