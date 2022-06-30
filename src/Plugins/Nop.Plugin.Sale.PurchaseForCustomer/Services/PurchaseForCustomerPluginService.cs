using System.Threading.Tasks;
using Nop.Services.Localization;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Services
{
    public class PurchaseForCustomerPluginService
    {
        private readonly ILocalizationService _localizationService;

        public PurchaseForCustomerPluginService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task AddOrUpdateResourcesAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Customers",
                "Customers");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Customers.Hint",
                "The customers that the product will be purchased for.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Store",
                "Store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Store.Hint",
                "The applicable store that the product will be purchased within.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid",
                "Mark order as paid");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid.Hint",
                "Mark the order(s) as paid immediately after the order(s) have been successfully placed.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer",
                "Notify customer");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer.Hint",
                "Notify each customer about the order");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Catalog.Products.PurchaseForCustomer",
                "Purchase for customer");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Catalog.Products.PurchaseForCustomer.Success",
                "Successfully manually placed order(s) for customers.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Catalog.Products.PurchaseForCustomer.Error",
                "Cannot manually placed order(s) for customers due to errors.");
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Customers");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Customers.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Store");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.Store.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Catalog.Products.PurchaseForCustomer");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Catalog.Products.PurchaseForCustomer.Error");
        }
    }
}
