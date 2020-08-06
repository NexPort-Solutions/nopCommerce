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

        public void AddOrUpdateResources()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Customers",
                "Customers");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Customers.Hint",
                "The customers that the product will be purchased for.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Store",
                "Store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Store.Hint",
                "The applicable store that the product will be purchased within.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid",
                "Mark order as paid");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid.Hint",
                "Mark the order(s) as paid immediately after the order(s) have been successfully placed.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer",
                "Notify customer");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer.Hint",
                "Notify each customer about the order");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Catalog.Products.PurchaseForCustomer",
                "Purchase for customer");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Catalog.Products.PurchaseForCustomer.Success",
                "Successfully manually placed order(s) for customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Admin.Catalog.Products.PurchaseForCustomer.Error",
                "Cannot manually placed order(s) for customers due to errors.");
        }

        public void DeleteResources()
        {
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Customers");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Customers.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Store");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.Store.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer");
            _localizationService.DeletePluginLocaleResource("Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer.Hint");
            _localizationService.DeletePluginLocaleResource("Admin.Catalog.Products.PurchaseForCustomer");
            _localizationService.DeletePluginLocaleResource("Admin.Catalog.Products.PurchaseForCustomer.Error");
        }
    }
}
