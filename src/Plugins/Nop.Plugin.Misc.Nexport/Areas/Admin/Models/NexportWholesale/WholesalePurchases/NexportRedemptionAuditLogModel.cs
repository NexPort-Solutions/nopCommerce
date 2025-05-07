using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;

public record NexportRedemptionAuditLogModel : BaseNopEntityModel
{
    public Guid InvoiceItemId { get; set; }

    public string Description { get; set; }

    public string CustomerDetails { get; set; }

    public string TargetedCustomerDetails { get; set; }

    public string Type { get; set; }

    public DateTime DateCreated { get; set; }
}
