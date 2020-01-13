using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours
{
    public class NexportDiscountPerCreditHoursPlugin : BasePlugin, IDiscountRequirementRule
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILocalizationService _localizationService;

        private readonly NexportService _nexportService;

        public NexportDiscountPerCreditHoursPlugin(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            ISettingService settingService,
            ILogger logger,
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            ILocalizationService localizationService,
            NexportService nexportService)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _discountService = discountService;
            _settingService = settingService;
            _logger = logger;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _localizationService = localizationService;
            _nexportService = nexportService;
        }

        public override void Install()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours", "Credit hours");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint", "Minimum credit hours for the discount to be effective");

            base.Install();
        }

        public override void Uninstall()
        {
            //discount requirements
            var discountRequirements = _discountService.GetAllDiscountRequirements()
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == NexportDiscountDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                _discountService.DeleteDiscountRequirement(discountRequirement);
            }

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint");

            base.Uninstall();
        }

        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var creditHours = _settingService.GetSettingByKey<decimal>(string.Format(NexportDiscountDefaults.SettingsKey, request.DiscountRequirementId));

            if (creditHours == 0M)
                return result;

            var totalHours = 0M;

            var shoppingCartItems = _shoppingCartService.GetShoppingCart(request.Customer, ShoppingCartType.ShoppingCart, request.Store.Id);
            foreach (var cartItem in shoppingCartItems)
            {
                var mapping = _nexportService.GetProductMappingByNopProductId(cartItem.ProductId, cartItem.StoreId);
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
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.CurrentRequestProtocol);
        }
    }
}
