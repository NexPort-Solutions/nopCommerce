using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportGroupProductRedemptionListSearchModel : BaseSearchModel
{
    public NexportGroupModel CurrentGroup { get; set; } = new() { OrganizationId = null, Name = "No Group" };

    public Product CurrentProduct { get; set; }

    [UIHint("Int32Nullable")]
    public int? OrderId {get; set; }

    public bool AdminView { get; set; } = false;

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchCustomerName")]
    public string SearchCustomerName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchCustomerEmail")]
    public string SearchEmail { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchPurchaserName")]
    public string SearchPurchaserName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchOrderNumber")]
    [UIHint("Int32Nullable")]
    public int? SearchOrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchStatus")]
    public NexportOrderInvoiceItemRedemptionStatus? SearchStatusId { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedFrom")]
    [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/DateNullable.cshtml")]
    public DateTime? DateAssignedFrom { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedTo")]
    [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/DateNullable.cshtml")]
    public DateTime? DateAssignedTo { get; set; }

    public NexportGroupProductRedemptionListSearchModel()
    {
        AvailableStatuses = new List<SelectListItem>();

        SetGridPageSize();
    }
}

public record NexportProductRedemptionListSearchModel : BaseSearchModel
{
    public int? FundingPoolId { get; set; }

    public NexportFundingPoolModel FundingPool { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchCustomerName")]
    public string SearchCustomerName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchCustomerEmail")]
    public string SearchCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchPurchaserName")]
    public string SearchPurchaserName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchOrderNumber")]
    [UIHint("Int32Nullable")]
    public int? SearchOrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchProductName")]
    public string SearchProductName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchStatus")]
    public NexportOrderInvoiceItemRedemptionStatus? SearchStatusId { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedFrom")]
    [UIHint("DateNullable")]
    public DateTime? DateAssignedFrom { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedTo")]
    [UIHint("DateNullable")]
    public DateTime? DateAssignedTo { get; set; }

    public NexportProductRedemptionListSearchModel()
    {
        AvailableStatuses = new List<SelectListItem>();

        SetGridPageSize();
    }
}