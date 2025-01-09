using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.Products;

public record NexportProductRedemptionStatusesModel : BaseNopModel
{
    [NopResourceDisplayName("Available")]
    public int Available { get; set; }

    [NopResourceDisplayName("Awaiting")]
    public int Awaiting { get; set; }

    [NopResourceDisplayName("Assigned")]
    public int Assigned { get; set; }

    public int ProductId { get; set;}
}