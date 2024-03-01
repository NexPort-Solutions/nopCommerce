using System.Threading.Tasks;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories;

public interface IPurchaseForCustomerModelFactory
{
    Task<PurchaseForCustomerPluginResourceListModel> PreparePurchaseForCustomerPluginResourceListModelAsync(
        PurchaseForCustomerPluginResourceListSearchModel searchModel);

    Task<PurchaseForCustomerOrderModel> PreparePurchaseForCustomerOrderModel(int productId);
}