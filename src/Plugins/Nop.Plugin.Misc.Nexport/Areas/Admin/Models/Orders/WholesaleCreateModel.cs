using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public class WholesaleCreateModel
{
    [NopResourceDisplayName("Admin.CurrentCarts.Store")]
    public int StoreId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Organization")]
    public Guid OrganizationId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.FundingPool")]
    public Guid FundingPoolId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedeemBy")]
    [UIHint("DateNullable")]
    public DateTime? RedeemByUtc { get; init; } = DateTime.Today;

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited")]
    public bool IsRedemptionPeriodUnlimited { get; init; }

    [NopResourceDisplayName("Admin.CurrentCarts.Quantity")]
    public int Quantity { get; init; }

    [NopResourceDisplayName("Admin.CurrentCarts.Product")]
    public int ProductId { get; init; }

    [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
    public string PaymentMethod { get; init; }

    public List<SelectListItem> AvailableOrganizations { get; init; } = new();
    public List<SelectListItem> AvailableFundingPools { get; init; } = new();
    public List<SelectListItem> AvailableStores { get; init; } = new();
    public List<SelectListItem> AvailableProducts { get; init; } = new();
    public List<SelectListItem> AvailablePaymentMethods { get; init; } = new();

    public string? Error { get; set; }
}
