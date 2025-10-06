using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Cms;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Components;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours;

public class NexportDiscountPerCreditHoursPlugin : BasePlugin, IWidgetPlugin, IDiscountRequirementRule
{
    private readonly NexportDiscountPerCreditHoursPluginService _pluginService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IDiscountService _discountService;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;
    private readonly IWebHelper _webHelper;
    private readonly IShoppingCartService _shoppingCartService;

    private readonly NexportService _nexportService;

    public NexportDiscountPerCreditHoursPlugin(
        NexportDiscountPerCreditHoursPluginService pluginService,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IDiscountService discountService,
        ISettingService settingService,
        ILogger logger,
        IWebHelper webHelper,
        IShoppingCartService shoppingCartService,
        NexportService nexportService)
    {
        _pluginService = pluginService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _discountService = discountService;
        _settingService = settingService;
        _logger = logger;
        _webHelper = webHelper;
        _shoppingCartService = shoppingCartService;
        _nexportService = nexportService;
    }

    public override async Task InstallAsync()
    {
        await _pluginService.AddOrUpdateResourcesAsync();

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

        await _pluginService.DeleteResourcesAsync();

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

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == null)
            throw new ArgumentNullException(nameof(widgetZone));

        if (widgetZone == AdminWidgetZones.PluginDetailsBottom)
            return typeof(WidgetsDiscountPerCreditHoursModifiedLocaleResourcesDataTableBlock);

        return null;
    }
}