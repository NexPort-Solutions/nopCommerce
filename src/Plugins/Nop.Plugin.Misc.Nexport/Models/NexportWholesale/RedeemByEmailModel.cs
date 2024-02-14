using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale
{
    public record RedeemByEmailModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.RedeemByEmailModel.ProductName")]
        public string? ProductName { get; set; }
        [NopResourceDisplayName("Plugins.Misc.Nexport.RedeemByEmailModel.InvoiceItemId")]
        public int InvoiceItemId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.Nexport.RedeemByEmailModel.RedeemedDate")]
        public DateTime RedeemedDate { get; set; }
        public Guid NexportUserId { get; set; }
        public int ProductMappingId { get; set; }
        public bool Redeemed { get; set; }
        [NopResourceDisplayName("Plugins.Misc.Nexport.RedeemByEmailModel.Status")]
        public NexportOrderInvoiceItemRedemptionStatus Status { get; set; }
        public Guid? EnrollmentId { get; set; }
    }
}
