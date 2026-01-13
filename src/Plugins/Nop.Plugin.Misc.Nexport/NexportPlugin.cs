using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.Nexport.Components;
using Nop.Plugin.Misc.Nexport.Infrastructure;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.Nexport;

public class NexportPlugin(
    NexportSettings nexportSettings,
    NexportPluginService nexportPluginService,
    IUrlHelperFactory urlHelperFactory,
    IActionContextAccessor actionContextAccessor,
    IAdminMenu adminMenu,
    IDiscountService discountService,
    WidgetSettings widgetSetting,
    ILocalizationService localizationService,
    IPermissionService permissionService,
    ISettingService settingService,
    IScheduleTaskService scheduleTaskService,
    IWebHelper webHelper,
    ILogger logger)
    : BasePlugin, IMiscPlugin, IWidgetPlugin
{
    private readonly IUrlHelperFactory _urlHelperFactory = urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor = actionContextAccessor;
    private readonly IDiscountService _discountService = discountService;
    private readonly IScheduleTaskService _scheduleTaskService = scheduleTaskService;
    private readonly ILogger _logger = logger;
    public override string GetConfigurationPageUrl()
    {
        return $"{webHelper.GetStoreLocation()}Admin/NexportIntegration/Configure";
    }

    public override async Task InstallAsync()
    {
        try
        {
            var migrationServiceProvider = PluginStartup.CreateFluentMigratorRunnerService();
            using var serviceScope = migrationServiceProvider.CreateScope();
            var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            try
            {
                runner.MigrateUp();
            }
            catch (MissingMigrationsException)
            {
                // ignored
            }
        }
        catch (Exception)
        {
            // Ignore
        }

        var settings = new NexportSettings();
        await settingService.SaveSettingAsync(settings);

        if (!widgetSetting.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
        {
            widgetSetting.ActiveWidgetSystemNames.Add(NexportDefaults.SystemName);
            await settingService.SaveSettingAsync(widgetSetting);
        }

        await nexportPluginService.AddMessageTemplatesAsync();

        await nexportPluginService.InstallScheduledTaskAsync();

        await nexportPluginService.AddActivityLogTypesAsync();

        await nexportPluginService.AddOrUpdateResourcesAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        if (widgetSetting.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
        {
            widgetSetting.ActiveWidgetSystemNames.Remove(NexportDefaults.SystemName);
            await settingService.SaveSettingAsync(widgetSetting);
        }

        await settingService.DeleteSettingAsync<NexportSettings>();

        await nexportPluginService.DeleteMessageTemplatesAsync();

        await nexportPluginService.UninstallScheduledTaskAsync();

        await nexportPluginService.DeleteActivityLogTypesAsync();

        await nexportPluginService.DeleteResourcesAsync();

        await nexportPluginService.UninstallPermissionProviderAsync();

        try
        {
            var migrationServiceProvider = PluginStartup.CreateFluentMigratorRunnerService();
            using var serviceScope = migrationServiceProvider.CreateScope();
            var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            try
            {
                runner.MigrateDown(0);

                ((MigrationRunner)runner).VersionLoader.RemoveVersionTable();
            }
            catch (MissingMigrationsException)
            {
                // ignored
            }
        }
        catch (Exception)
        {
            // Ignore
        }

        await base.UninstallAsync();
    }

    public bool HideInWidgetList => true;

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.StoreDetailsBottom,
                AdminWidgetZones.ProductDetailsButtons,
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.CustomerDetailsButtons,
                AdminWidgetZones.CustomerDetailsBlock,
                AdminWidgetZones.CustomerUserDetailsBlock,
                AdminWidgetZones.CategoryDetailsBlock,
                AdminWidgetZones.OrderDetailsBlock,
                AdminWidgetZones.PluginDetailsBottom,
                PublicWidgetZones.OrderDetailsProductLine,
                PublicWidgetZones.AccountNavigationAfter,
                PublicWidgetZones.HeaderLinksBefore,
                PublicWidgetZones.OrderSummaryCartFooter,
                PublicWidgetZones.ProductDetailsOverviewTop,
                NexportDefaults.NexportRegistrationFieldsZone,
                AdminWidgetZones.OrderListButtons,
                PublicWidgetZones.OrderDetailsPageAfterproducts,
                PublicWidgetZones.ProductDetailsAfterPictures,
                PublicWidgetZones.ProductBoxAddinfoBefore,
                PublicWidgetZones.ProductDetailsAfterBreadcrumb
            });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == null)
            throw new ArgumentNullException(nameof(widgetZone));

        if (widgetZone == AdminWidgetZones.PluginDetailsBottom)
            return typeof(WidgetsNexportModifiedLocaleResourcesDataTableBlock);

        if (widgetZone == AdminWidgetZones.StoreDetailsBottom)
            return typeof(WidgetsNexportStoreDetails);

        if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
            return typeof(WidgetsNexportProductMappingsInProductPage);

        if (widgetZone == AdminWidgetZones.ProductDetailsButtons)
            return typeof(WidgetsNexportProductDetailsButtons);

        if (widgetZone == AdminWidgetZones.CustomerDetailsButtons)
            return typeof(WidgetsNexportCustomerDetailsButtons);

        if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
            return typeof(WidgetsNexportCustomerDetailsBlock);

        if (widgetZone == AdminWidgetZones.CustomerUserDetailsBlock)
            return typeof(WidgetsNexportCustomerUserDetailsBlock);

        if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
            return typeof(WidgetsNexportCategoryDetailsBlock);

        if (widgetZone == AdminWidgetZones.OrderDetailsBlock)
            return typeof(WidgetsNexportOrderDetailsBlock);

        if (widgetZone == PublicWidgetZones.OrderDetailsProductLine)
            return typeof(WidgetsNexportOrderDetailsProductLine);

        if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
            return typeof(WidgetsAccountNavigationAfter);

        if (widgetZone == PublicWidgetZones.HeaderLinksBefore)
            return typeof(WidgetsHeaderLinksBefore);

        if (widgetZone == PublicWidgetZones.OrderSummaryCartFooter)
            return typeof(WidgetsOrderSummaryCartFooter);

        if (widgetZone == PublicWidgetZones.ProductDetailsAfterBreadcrumb)
            return typeof(WidgetsNexportProductDetailsAfterBreadcrumb);

        if (widgetZone == PublicWidgetZones.ProductDetailsOverviewTop)
            return typeof(WidgetsProductDetailsOverviewTop);

        if (widgetZone == NexportDefaults.NexportRegistrationFieldsZone)
            return typeof(WidgetsNexportRegistrationFields);

        if (widgetZone == AdminWidgetZones.OrderListButtons)
            return typeof(WidgetsNexportOrderListButtons);

        if (widgetZone == PublicWidgetZones.OrderDetailsPageAfterproducts)
            return typeof(WidgetsNexportOrderDetailsPageAfterproducts);

        if (widgetZone == PublicWidgetZones.ProductDetailsAfterPictures)
            return typeof(WidgetsNexportProductDetailsAfterPictures);

        if (widgetZone == PublicWidgetZones.ProductBoxAddinfoBefore)
            return typeof(WidgetsNexportProductBoxAddInfoBefore);

        return null;
    }
}