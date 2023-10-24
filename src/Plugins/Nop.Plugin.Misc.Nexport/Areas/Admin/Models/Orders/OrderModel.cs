namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public record OrderModel : Web.Areas.Admin.Models.Orders.OrderModel
{
    public string? StoreUrl { get; set; }
}
