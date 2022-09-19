using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Orders;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Services
{
    public interface IPluginLocalizationService
    {
        Task AddOrUpdateResourceWhenNonExisting(string resourceName, string resourceValue,
            string languageCulture = null);

        Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue,
            int languageId = 1);
    }
}
