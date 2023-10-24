using System;
using System.Linq;
using System.Threading.Tasks;
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

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours;

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

    private readonly INexportService _nexportService;
    private readonly IProductMappingService _productMappingService;

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
        INexportService nexportService,
        IProductMappingService productMappingService)
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
        _productMappingService = productMappingService;
    }

    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours", "Credit hours");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint", "Minimum credit hours for the discount to be effective");
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        //discount requirements
        var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
            .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == NexportDiscountDefaults.SYSTEM_NAME);
        foreach (var discountRequirement in discountRequirements)
        {
            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, true);
        }
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.Misc.Nexport.DiscountPerCreditHours.Fields.CreditHours.Hint");
        await base.UninstallAsync();
    }

    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Invalid by default
        var result = new DiscountRequirementValidationResult();

        if (request.Customer is null)
        {
            return result;
        }

        var creditHours = await _settingService.GetSettingByKeyAsync<decimal>(
            string.Format(NexportDiscountDefaults.SETTINGS_KEY, request.DiscountRequirementId));

        if (creditHours == 0M)
        {
            return result;
        }

        var totalHours = 0M;

        var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(request.Customer, ShoppingCartType.ShoppingCart, request.Store.Id);
        foreach (var cartItem in shoppingCartItems)
        {
            var mapping = await _productMappingService.GetByNopProductId(cartItem.ProductId, cartItem.StoreId) ??
                          await _productMappingService.GetByNopProductId(cartItem.ProductId);
            if (mapping?.CreditHours is not null)
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
}
