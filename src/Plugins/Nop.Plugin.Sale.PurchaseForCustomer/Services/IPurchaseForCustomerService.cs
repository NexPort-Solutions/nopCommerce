using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Orders;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Services
{
    public interface IPurchaseForCustomerService
    {
        PlaceOrderResult PurchaseProductForCustomer(Product product, Customer customer, Store store,
            bool notifyCustomer = false);
    }
}
