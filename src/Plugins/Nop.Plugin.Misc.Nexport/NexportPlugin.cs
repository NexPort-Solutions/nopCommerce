using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;
using Nop.Plugin.Misc.Nexport.Extensions;
using AdminController = Nop.Web.Areas.Admin.Controllers;
using Name = Nop.Plugin.Misc.Nexport.SystemNames;
using Title = Nop.Plugin.Misc.Nexport.SiteNodeTitles;
using Nop.Plugin.Misc.Nexport.Components.Widgets;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;
using Nop.Plugin.Misc.Nexport.Components.Widgets.Customer;
using Nop.Plugin.Misc.Nexport.Components.Widgets.Product;
using Nop.Plugin.Misc.Nexport.Components.Widgets.Store;
using Nop.Plugin.Misc.Nexport.Components.Widgets.Order;
using Nop.Plugin.Misc.Nexport.Components.Widgets.Category;

namespace Nop.Plugin.Misc.Nexport;

public class NexportPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
{
    private readonly Settings _settings;
    private readonly Services.PluginService _plugin;
    private readonly IPermissionService _permission;
    private readonly ISettingService _setting;
    private readonly WidgetSettings _widgetSettings;
    private readonly IWebHelper _webHelper;
    private readonly ILogger _logger;

    private readonly Dictionary<string, Type> _widgetMapping = new()
    {
        { AdminWidgetZones.StoreDetailsBottom, typeof(StoreDetails) },
        { AdminWidgetZones.ProductDetailsBlock, typeof(ProductMappingsInProductPage) },
        { AdminWidgetZones.ProductDetailsButtons, typeof(ProductDetailsButtons) },
        { AdminWidgetZones.CustomerDetailsButtons, typeof(CustomerDetailsButtons) },
        { AdminWidgetZones.CustomerDetailsBlock, typeof(CustomerDetailsBlock) },
        { AdminWidgetZones.CustomerUserDetailsBlock, typeof(CustomerUserDetailsBlock) },
        { AdminWidgetZones.CategoryDetailsBlock, typeof(CategoryDetails) },
        { AdminWidgetZones.OrderDetailsBlock, typeof(OrderDetails) },
        { PublicWidgetZones.OrderDetailsProductLine, typeof(OrderDetailsProductLine) },
        { PublicWidgetZones.AccountNavigationAfter, typeof(SettingNavigation) },
        { PublicWidgetZones.HeaderLinksBefore, typeof(HeaderLinksBefore) },
        { PublicWidgetZones.OrderSummaryCartFooter, typeof(SummaryCartFooter) },
        { PublicWidgetZones.ProductDetailsOverviewTop, typeof(ProductDetailsOverviewTop) },
        { Defaults.RegistrationFieldsZone, typeof(RegistrationFields) },
        { AdminWidgetZones.OrderListButtons, typeof(Wholesale) },
    };

    private const string SITE_MAP_NODE_ICON_CLASS = "far fa-dot-circle";
    private const string SYSTEM_NODE_ICON_CLASS = "fas fa-plug";

    public NexportPlugin(
        Settings settings,
        Services.PluginService pluginService,
        WidgetSettings widgetSetting,
        IPermissionService permissionService,
        ISettingService settingService,
        IWebHelper webHelper,
        ILogger logger)
    {
        _settings = settings;
        _plugin = pluginService;
        _widgetSettings = widgetSetting;
        _permission = permissionService;
        _setting = settingService;
        _webHelper = webHelper;
        _logger = logger;
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(siteMapNode => siteMapNode.SystemName == Name.NEXPORT);
        if (pluginNode is not null)
        {
            return;
        }
        if (_settings is null || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return;
        }
        var systemNode = new SiteMapNode
        {
            SystemName = Name.NEXPORT,
            Visible = true,
            Title = Title.NEXPORT,
            IconClass = SYSTEM_NODE_ICON_CLASS,
        };
        systemNode.ChildNodes.Add(new SiteMapNode
            {
                Visible = await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins),
                Title = Title.CONFIGURATION,
                SystemName = Name.INTEGRATION_CONFIGURATION,
                ControllerName = ViewUtilities.GetControllerName<IntegrationController>(),
                ActionName = nameof(IntegrationController.Configure),
                IconClass = SITE_MAP_NODE_ICON_CLASS,
            });
        systemNode.ChildNodes.Add(new SiteMapNode
            {
                Visible = await _permission.AuthorizeAsync(StandardPermissionProvider.ManageStores),
                Title = Title.STORE_CONFIGURATION,
                SystemName = Name.STORE_CONFIGURATION,
                ControllerName = ViewUtilities.GetControllerName<AdminController.StoreController>(),
                ActionName = nameof(AdminController.StoreController.List),
                IconClass = SITE_MAP_NODE_ICON_CLASS,
            });
        systemNode.ChildNodes.Add(new SiteMapNode
            {
                Visible = await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo),
                Title = Title.SUPPLEMENTAL_INFO,
                SystemName = Name.SUPPLEMENTAL_INFO,
                ControllerName = ViewUtilities.GetControllerName<SupplementalInfoController>(),
                ActionName = nameof(SupplementalInfoController.ListSupplementalInfoQuestion),
                IconClass = SITE_MAP_NODE_ICON_CLASS,
            });
        systemNode.ChildNodes.Add(new SiteMapNode
            {
                Visible = await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions),
                Title = Title.GROUPS,
                SystemName = Name.GROUPS,
                ControllerName = ViewUtilities.GetControllerName<WholesaleController>(),
                ActionName = nameof(WholesaleController.AdminGroups),
                IconClass = SITE_MAP_NODE_ICON_CLASS,
            });
        rootNode.ChildNodes.Add(systemNode);
    }

    public override string GetConfigurationPageUrl() => $"{_webHelper.GetStoreLocation()}Admin/RegistrationField/Configure";

    public override async Task InstallAsync()
    {
        var migrationServiceProvider = PluginStartup.CreateFluentMigratorRunnerService();
        using var serviceScope = migrationServiceProvider.CreateScope();
        var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        try
        {
            runner.MigrateUp();
        }
        catch (MissingMigrationsException) { }
        var settings = new Settings();
        await _setting.SaveSettingAsync(settings);
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Name.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(Name.SYSTEM_NAME);
            await _setting.SaveSettingAsync(_widgetSettings);
        }
        await _plugin.AddMessageTemplatesAsync();
        await _plugin.InstallScheduledTaskAsync();
        await _plugin.AddActivityLogTypesAsync();
        await _plugin.AddOrUpdateResourcesAsync();
        await _plugin.InstallPermissionProviderAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(Name.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(Name.SYSTEM_NAME);
            await _setting.SaveSettingAsync(_widgetSettings);
        }
        await _setting.DeleteSettingAsync<Settings>();
        await _plugin.DeleteMessageTemplatesAsync();
        await _plugin.UninstallScheduledTaskAsync();
        await _plugin.DeleteActivityLogTypesAsync();
        await _plugin.DeleteResourcesAsync();
        await _plugin.UninstallPermissionProviderAsync();
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
            catch (MissingMigrationsException) { }
        }
#pragma warning disable
        catch (Exception) { }
#pragma warning restore
        await base.UninstallAsync();
    }

    public bool HideInWidgetList => true;

    public Task<IList<string>> GetWidgetZonesAsync() => Task.FromResult<IList<string>>(_widgetMapping.Keys.ToList());
    public Type? GetWidgetViewComponent(string widgetZone) => _widgetMapping.TryGetValue(widgetZone, out var result) ? result : null;
}
