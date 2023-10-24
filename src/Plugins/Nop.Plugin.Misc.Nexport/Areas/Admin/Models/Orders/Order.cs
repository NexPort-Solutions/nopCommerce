namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

internal class Order : Core.Domain.Orders.Order
{
    public bool IsWholesale { get; init; }
    public Guid OrganizationId { get; init; }
}
