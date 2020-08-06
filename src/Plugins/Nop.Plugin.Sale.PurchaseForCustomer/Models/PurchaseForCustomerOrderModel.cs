using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Models
{
    public class PurchaseForCustomerOrderModel
    {
        public PurchaseForCustomerOrderModel()
        {
            CustomerIds = new List<int>();
            AvailableCustomers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
        }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.Customers")]
        public IList<int> CustomerIds { get; set; }

        [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid")]
        public bool MarkOrderAsPaid { get; set; }

        [NopResourceDisplayName("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer")]
        public bool NotifyCustomer { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
