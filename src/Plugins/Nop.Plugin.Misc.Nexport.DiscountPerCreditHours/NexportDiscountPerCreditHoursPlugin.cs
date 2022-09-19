using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours
{
    public class NexportDiscountPerCreditHoursPlugin : BasePlugin, IWidgetPlugin, IDiscountRequirementRule
    {
        private readonly NexportDiscountPerCreditHoursPluginService _nexportDiscountPerCreditHoursPluginService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginLocalizationService _pluginLocalizationService;
        private readonly WidgetSettings _widgetSettings;

        private readonly NexportService _nexportService;

        
        public NexportDiscountPerCreditHoursPlugin(
            NexportDiscountPerCreditHoursPluginService nexportDiscountPerCreditHoursPluginService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            ISettingService settingService,
            ILogger logger,
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            ILocalizationService localizationService,
            IPluginLocalizationService pluginLocalizationService,
            WidgetSettings widgetSettings,
            NexportService nexportService)
        {
            _nexportDiscountPerCreditHoursPluginService = nexportDiscountPerCreditHoursPluginService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _discountService = discountService;
            _settingService = settingService;
            _logger = logger;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _localizationService = localizationService;
            _pluginLocalizationService = pluginLocalizationService;
            _widgetSettings = widgetSettings;
            _nexportService = nexportService;
        }

        public override async Task InstallAsync()
        {
            await _nexportDiscountPerCreditHoursPluginService.AddOrUpdateResourcesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //discount requirements
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == NexportDiscountDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, true);
            }

            await _nexportDiscountPerCreditHoursPluginService.DeleteResourcesAsync();

           await base.UninstallAsync();
        }

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var creditHours = await _settingService.GetSettingByKeyAsync<decimal>(
                string.Format(NexportDiscountDefaults.SettingsKey, request.DiscountRequirementId));

            if (creditHours == 0M)
                return result;

            var totalHours = 0M;

            var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(request.Customer, ShoppingCartType.ShoppingCart, request.Store.Id);
            foreach (var cartItem in shoppingCartItems)
            {
                var mapping = await _nexportService.GetProductMappingByNopProductId(cartItem.ProductId, cartItem.StoreId) ??
                              await _nexportService.GetProductMappingByNopProductId(cartItem.ProductId);
                if (mapping?.CreditHours != null)
                {
                    totalHours += mapping.CreditHours.Value * cartItem.Quantity;
                }
            }

            result.IsValid = totalHours >= creditHours;
            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "NexportDiscountPerCreditHours",
                new { discountId, discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        public bool HideInWidgetList => true;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(
                new List<string>
                {
                    AdminWidgetZones.PluginDetailsBottom
                });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.PluginDetailsBottom)
                return "WidgetsDiscountPerCreditHoursModifiedLocaleResourcesDataTableBlock";

            return "";
        }
    }
}
