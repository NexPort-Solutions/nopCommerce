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

    [NopResourceDisplayName("Plugins.Misc.Nexport.Organization")]
    public Guid OrganizationId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.RedeemBy")]
    [UIHint("DateNullable")]
    public DateTime? RedeemByUtc { get; set; }

    //[NopResourceDisplayName("Plugins.Misc.Nexport.Wholesale.IsRedemptionPeriodUnlimited")]
    //public bool IsRedemptionPeriodUnlimited { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
    public string PaymentMethod { get; set; }


    public Guid? FundingPoolId { get; set; }

    public List<SelectListItem> AvailableStores { get; set; } = new();
    public List<SelectListItem> AvailablePaymentMethods { get; set; } = new();

    public List<SelectListItem> AvailableOrganizations { get; set; } = new();
    public List<SelectListItem> AvailableFundingPools { get; set; } = new();

    public string Error { get; set; }
}