using Nop.Plugin.Sale.PurchaseForCustomer.Models;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories
{
    public interface IPurchaseForCustomerModelFactory
    {
        PurchaseForCustomerOrderModel PreparePurchaseForCustomerOrderModel(int productId);
    }
}
