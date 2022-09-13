using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Services.Discounts;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Infrastructure;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Nexport
{
    public class NexportPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;
        private readonly NexportSettings _nexportSettings;
        private readonly NexportPluginService _nexportPluginService;

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        public NexportPlugin(
            IRepository<NexportProductMapping> nexportProductRepository,
            NexportSettings nexportSettings,
            NexportPluginService nexportPluginService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            WidgetSettings widgetSetting,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper, ILogger logger)
        {
            _nexportProductRepository = nexportProductRepository;

            _nexportSettings = nexportSettings;
            _nexportPluginService = nexportPluginService;

            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;

            _discountService = discountService;
            _widgetSettings = widgetSetting;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
            _logger = logger;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Nexport");
            if (pluginNode != null)
                return;

            if (_nexportSettings == null || string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return;

            var node = new SiteMapNode()
            {
                SystemName = "Nexport",
                Visible = true,
                Title = "Nexport Integration",
                IconClass = "fas fa-plug",
            };

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins),
                Title = "Configuration",
                SystemName = "Nexport Integration - Configuration",
                ControllerName = "NexportIntegration",
                ActionName = "Configure",
                IconClass = "far fa-dot-circle"
            });

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores),
                Title = "Store Configuration",
                SystemName = "Nexport Integration - Store Configuration",
                ControllerName = "Store",
                ActionName = "List",
                IconClass = "far fa-dot-circle"
            });

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageSupplementalInfo),
                Title = "Supplemental Info",
                SystemName = NexportDefaults.SUPPLEMENTAL_INFO_MENU_SYSTEM_NAME,
                ControllerName = "NexportIntegration",
                ActionName = "ListSupplementalInfoQuestion",
                IconClass = "far fa-dot-circle"
            });

            rootNode.ChildNodes.Add(node);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/NexportIntegration/Configure";
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
            await _settingService.SaveSettingAsync(settings);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(NexportDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _nexportPluginService.AddMessageTemplatesAsync();

            await _nexportPluginService.InstallScheduledTaskAsync();

            await _nexportPluginService.AddActivityLogTypesAsync();

            await _nexportPluginService.AddOrUpdateResourcesAsync();

            await _nexportPluginService.InstallPermissionProviderAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(NexportDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<NexportSettings>();

            await _nexportPluginService.DeleteMessageTemplatesAsync();

            await _nexportPluginService.UninstallScheduledTaskAsync();

            await _nexportPluginService.DeleteActivityLogTypesAsync();

            await _nexportPluginService.DeleteResourcesAsync();

            await _nexportPluginService.UninstallPermissionProviderAsync();

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
                PublicWidgetZones.OrderDetailsProductLine,
                PublicWidgetZones.AccountNavigationAfter,
                PublicWidgetZones.HeaderLinksBefore,
                PublicWidgetZones.OrderSummaryCartFooter,
                PublicWidgetZones.ProductDetailsOverviewTop,
                NexportDefaults.NexportRegistrationFieldsZone
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.StoreDetailsBottom)
                return "WidgetsNexportStoreDetails";

            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return "WidgetsNexportProductMappingsInProductPage";

            if (widgetZone == AdminWidgetZones.ProductDetailsButtons)
                return "WidgetsNexportProductDetailsButtons";

            if (widgetZone == AdminWidgetZones.CustomerDetailsButtons)
                return "WidgetsNexportCustomerDetailsButtons";

            if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                return "WidgetsNexportCustomerDetailsBlock";

            if (widgetZone == AdminWidgetZones.CustomerUserDetailsBlock)
                return "WidgetsNexportCustomerUserDetailsBlock";

            if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
                return "WidgetsNexportCategoryDetailsBlock";

            if (widgetZone == AdminWidgetZones.OrderDetailsBlock)
                return "WidgetsNexportOrderDetailsBlock";

            if (widgetZone == PublicWidgetZones.OrderDetailsProductLine)
                return "WidgetsNexportOrderDetailsProductLine";

            if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
                return "WidgetsAccountNavigationAfter";

            if (widgetZone == PublicWidgetZones.HeaderLinksBefore)
                return "WidgetsHeaderLinksBefore";

            if (widgetZone == PublicWidgetZones.OrderSummaryCartFooter)
                return "WidgetsOrderSummaryCartFooter";

            if (widgetZone == PublicWidgetZones.ProductDetailsOverviewTop)
                return "WidgetsProductDetailsOverviewTop";

            if (widgetZone == NexportDefaults.NexportRegistrationFieldsZone)
                return "WidgetsNexportRegistrationFields";

            return "";
        }
    }
}
