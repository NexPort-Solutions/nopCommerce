using System.Linq;

using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using LinqToDB;
using System.Collections.Generic;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Services
{
    public class PurchaseForCustomerPluginService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPluginLocalizationService _pluginLocalizationService;
        private readonly ISettingService _settingService;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IGenericAttributeService _genericAttributeService;

        private static readonly Dictionary<string, string> _localeResources = new()
        {
            {"Plugins.Sale.Nexport.PurchaseForCustomer.Customers",
                "Customers"},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.Customers.Hint",
                "The customers that the product will be purchased for."},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.Store", "Store"},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.Store.Hint",
                "The applicable store that the product will be purchased within."},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid",
                "Mark order as paid"},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.MarkOrderAsPaid.Hint",
                "Mark the order(s) as paid immediately after the order(s) have been successfully placed."},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer",
                "Notify customer"},
            {"Plugins.Sale.Nexport.PurchaseForCustomer.NotifyCustomer.Hint",
                "Notify each customer about the order"},
            {"Admin.Catalog.Products.PurchaseForCustomer",
                "Purchase for customer"},
            {"Admin.Catalog.Products.PurchaseForCustomer.Success",
                "Successfully manually placed order(s) for customers."},
            {"Admin.Catalog.Products.PurchaseForCustomer.Error",
                "Cannot manually placed order(s) for customers due to errors."},
            {"Plugins.Sale.PurchaseForCustomer.Errors.ModifiedLocaleResources",
                "There are modified resource values within the <b>Purchase for customer plugin</b> that conflicts with the default value. Review them <a href=\"javascript:OpenWindow(\'{0}\', 800, 500, true)\">here</a>."}
        };

        public static Dictionary<string, string> GetLocaleResource()
        {
            return _localeResources;
        }

        public PurchaseForCustomerPluginService(ILocalizationService localizationService, 
            IPluginLocalizationService pluginLocalizationService,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            IRepository<GenericAttribute> genericAttributeRepository)
        {
            _localizationService = localizationService;
            _pluginLocalizationService = pluginLocalizationService;
            _settingService = settingService;
            _genericAttributeService = genericAttributeService;
            _genericAttributeRepository = genericAttributeRepository;
        }

        public async Task AddOrUpdateResourceAsync(string resourceName, string resourceValue, int languageId = 1)
        {
            // if this returns back false, then it didn't exist
            if (await _pluginLocalizationService.CheckForExistingResourceAndAddNonExistingResource(resourceName, resourceValue))
            {
                // this is if we have multiple modified values
                var pfcSetting =
                    await _settingService.GetSettingAsync("Plugin.Sale.PurchaseForCustomer.HasModifiedLocaleResources");
              
                if (pfcSetting == null)
                {
                    await _settingService.SetSettingAsync<bool>("Plugin.Sale.PurchaseForCustomer.HasModifiedLocaleResources", true);
                }
            }
        }

        public async Task AddOrUpdateResourcesAsync()
        {
            foreach (var localeResource in _localeResources)
            {
                await AddOrUpdateResourceAsync(localeResource.Key, localeResource.Value);
            }
        }

        public async Task DeleteResourcesAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync(_localeResources.Keys.ToList());
        }
    }
}
