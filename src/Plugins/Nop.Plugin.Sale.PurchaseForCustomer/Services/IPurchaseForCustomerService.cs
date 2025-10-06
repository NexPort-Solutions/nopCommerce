using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Orders;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Services;

public interface IPurchaseForCustomerService
{
    Task<PlaceOrderResult> PurchaseProductForCustomerAsync(Product product, Customer customer, Store store,
        DateTime? utcStartDate = null, bool notifyCustomer = false);

    Task<IList<Customer>> SearchCustomersAsync(string searchNameAndEmail);
}