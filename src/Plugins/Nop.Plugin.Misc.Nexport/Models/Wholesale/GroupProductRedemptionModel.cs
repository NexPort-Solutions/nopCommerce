using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record GroupProductRedemptionModel : BaseNopEntityModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = "Available";
    public Guid InvoiceItemId { get; set; }
}
