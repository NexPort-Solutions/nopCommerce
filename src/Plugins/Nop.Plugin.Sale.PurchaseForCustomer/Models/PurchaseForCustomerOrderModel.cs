using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Models;

public record PurchaseForCustomerOrderModel
{
    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.Customers")]
    public IList<int> CustomerIds { get; set; } = new List<int>();

    [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.Store")]
    public int StoreId { get; set; }

    [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid")]
    public bool MarkOrderAsPaid { get; set; }

    [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.StartDate")]
    [UIHint("DateTimeNullable")]
    public DateTime? StartDate { get; set; }

    [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer")]
    public bool NotifyCustomer { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
}