using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using FluentMigrator.Runner;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Services.Discounts;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport {
    public class NexportPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly NexportPluginObjectContext _nexportProductMappingContext;
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;
        private readonly NexportSettings _nexportSettings;
        private readonly NexportPluginService _nexportPluginService;

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        public NexportPlugin(
            NexportPluginObjectContext nexportProductMappingContext,
            IRepository<NexportProductMapping> nexportProductRepository,
            NexportSettings nexportSettings,
            NexportPluginService nexportPluginService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            WidgetSettings widgetSetting,
            ILocalizationService localizationService,
            ISettingService settingService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper, ILogger logger)
        {
            _nexportProductMappingContext = nexportProductMappingContext;
            _nexportProductRepository = nexportProductRepository;

            _nexportSettings = nexportSettings;
            _nexportPluginService = nexportPluginService;

            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;

            _discountService = discountService;
            _widgetSettings = widgetSetting;
            _localizationService = localizationService;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
            _logger = logger;
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Nexport");
            if (pluginNode != null) return;

            if (_nexportSettings == null || string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return;

            var node = new SiteMapNode()
            {
                SystemName = "Nexport",
                Visible = true,
                Title = "Nexport Integration",
                IconClass = "fa-plug",
            };

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = true,
                Title = "Configuration",
                SystemName = "Nexport Integration - Configuration",
                ControllerName = "NexportIntegration",
                ActionName = "Configure",
                IconClass = "fa fa-cog"
            });

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = true,
                Title = "Store Configuration",
                SystemName = "Nexport Integration - Store Configuration",
                ControllerName = "Store",
                ActionName = "List",
                IconClass = "fa fa-cog"
            });

            node.ChildNodes.Add(new SiteMapNode()
            {
                Visible = true,
                Title = "Supplemental Info",
                SystemName = NexportDefaults.SUPPLEMENTAL_INFO_MENU_SYSTEM_NAME,
                ControllerName = "NexportIntegration",
                ActionName = "ListSupplementalInfoQuestion",
                IconClass = "fa fa-cog"
            });

            rootNode.ChildNodes.Add(node);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/NexportIntegration/Configure";
        }

        public override void Install()
        {
            _nexportProductMappingContext.Install();

            var settings = new NexportSettings();
            _settingService.SaveSetting(settings);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(NexportDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _nexportPluginService.InstallScheduledTask();

            _nexportPluginService.AddActivityLogTypes();

            _nexportPluginService.AddOrUpdateResources();

            base.Install();
        }

        public override void Uninstall()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(NexportDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(NexportDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _settingService.DeleteSetting<NexportSettings>();

            _nexportPluginService.UninstallScheduledTask();

            _nexportPluginService.DeleteActivityLogTypes();

            _nexportPluginService.DeleteResources();

            try
            {
                var runner = EngineContext.Current.Resolve<IMigrationRunner>();
                runner.MigrateDown(0);

                ((MigrationRunner)runner).VersionLoader.RemoveVersionTable();
            }
            catch (Exception)
            {
                // Ignore
            }

            _nexportProductMappingContext.Uninstall();

            base.Uninstall();
        }

        public bool HideInWidgetList => true;

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                AdminWidgetZones.StoreDetailsBottom,
                AdminWidgetZones.ProductDetailsButtons,
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.CustomerDetailsBlock,
                AdminWidgetZones.CustomerUserDetailsBlock,
                AdminWidgetZones.CategoryDetailsBlock,
                PublicWidgetZones.OrderDetailsProductLine,
                PublicWidgetZones.AccountNavigationAfter,
                PublicWidgetZones.HeaderLinksBefore,
                PublicWidgetZones.OrderSummaryCartFooter,
                PublicWidgetZones.ProductDetailsOverviewTop,
                NexportDefaults.NexportRegistrationFieldsZone
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.StoreDetailsBottom)
                return "WidgetsNexportStoreDetails";

            if(widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return "WidgetsNexportProductMappingsInProductPage";

            if (widgetZone == AdminWidgetZones.ProductDetailsButtons)
                return "WidgetsNexportProductDetailsButtons";

            if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                return "WidgetsNexportCustomerDetailsBlock";

            if (widgetZone == AdminWidgetZones.CustomerUserDetailsBlock)
                return "WidgetsNexportCustomerUserDetailsBlock";

            if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
                return "WidgetsNexportCategoryDetailsBlock";

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
