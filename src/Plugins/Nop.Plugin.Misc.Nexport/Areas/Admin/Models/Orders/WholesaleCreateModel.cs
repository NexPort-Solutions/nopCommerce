using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public class WholesaleCreateModel
{
    [NopResourceDisplayName("Admin.CurrentCarts.Store")]
    public int StoreId { get; init; }

    [NopResourceDisplayName("Admin.CurrentCarts.Product")]
    public int ProductId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedeemBy")]
    [UIHint("DateNullable")]
    public DateTime? RedeemByUtc { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited")]
    public bool IsRedemptionPeriodUnlimited { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.Purchasing.Agent")]
    public int AgentId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.Purchasing.Group")]
    public Guid GroupId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.ApplyGroupMembershipWhenRedeemed")]
    public bool ApplyGroupMembershipWhenRedeemed { get; init; }

    [NopResourceDisplayName("Admin.CurrentCarts.Quantity")]
    public int Quantity { get; init; }

    [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
    public string? PaymentMethod { get; init; }
}
