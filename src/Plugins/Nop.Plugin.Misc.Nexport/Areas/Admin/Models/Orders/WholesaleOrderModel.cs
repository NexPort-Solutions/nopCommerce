using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public class WholesaleOrderModel
{
    [NopResourceDisplayName("Admin.CurrentCarts.Store")]
    public int StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Order.GroupId")]
    [UIHint("GuidNullable")]
    public Guid? OrganizationId { get; set; }

    public string OrganizationName { get; set; }

    public string OrganizationShortName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedeemBy")]
    [UIHint("DateNullable")]
    public DateTime? RedeemByUtc { get; set; }

    //[NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited")]
    //public bool IsRedemptionPeriodUnlimited { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
    public string PaymentMethod { get; set; }

    [UIHint("Int32Nullable")]
    public int? FundingPoolId { get; set; }

    public List<SelectListItem> AvailableStores { get; set; } = [];

    public List<SelectListItem> AvailablePaymentMethods { get; set; } = [];

    public List<SelectListItem> AvailableOrganizations { get; set; } = [];

    public List<SelectListItem> AvailableFundingPools { get; set; } = [];

    public string Error { get; set; }
}