using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests
{
    public class CancelPendingOrderRequestsPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly CancelPendingOrderRequestsPluginService _cancelCancelPendingOrderRequestsPluginService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public CancelPendingOrderRequestsPlugin(
            CancelPendingOrderRequestsPluginService cancelCancelPendingOrderRequestsPluginService,
            WidgetSettings widgetSetting,
            ISettingService settingService)
        {
            _cancelCancelPendingOrderRequestsPluginService = cancelCancelPendingOrderRequestsPluginService;
            _widgetSettings = widgetSetting;
            _settingService = settingService;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var salesNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Sales");
            if (salesNode == null)
                return;

            var cancelOrderRequestNode = new SiteMapNode()
            {
                SystemName = "Cancellation requests",
                Title = "Cancellation requests",
                ControllerName = "CancelPendingOrderRequests",
                ActionName = "List",
                IconClass = "fa fa-dot-circle-o",
                Visible = true
            };

            salesNode.ChildNodes.Add(cancelOrderRequestNode);
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
                    ((MigrationRunner)runner).MigrateUp();
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

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _cancelCancelPendingOrderRequestsPluginService.AddActivityLogTypesAsync();
            await _cancelCancelPendingOrderRequestsPluginService.AddMessageTemplatesAsync();
            await _cancelCancelPendingOrderRequestsPluginService.AddOrUpdateResourcesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _cancelCancelPendingOrderRequestsPluginService.DeleteMessageTemplatesAsync();
            await _cancelCancelPendingOrderRequestsPluginService.DeleteActivityLogTypesAsync();
            await _cancelCancelPendingOrderRequestsPluginService.DeleteResourcesAsync();

            try
            {
                var migrationServiceProvider = PluginStartup.CreateFluentMigratorRunnerService();
                using var serviceScope = migrationServiceProvider.CreateScope();
                var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                try
                {
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

            var versionSetting = await _settingService.GetSettingAsync(PluginDefaults.ASSEMBLY_VERSION_KEY);
            if (versionSetting != null)
            {
                await _settingService.DeleteSettingAsync(versionSetting);
            }

            await base.UninstallAsync();
        }

        public bool HideInWidgetList => true;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(
                new List<string>
                {
                    PublicWidgetZones.OrderDetailsPageOverview,
                    AdminWidgetZones.OrderSettingsDetailsBlock
                });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.OrderDetailsPageOverview)
            {
                return "WidgetsOrderDetailsPageOverview";
            }

            if (widgetZone == AdminWidgetZones.OrderSettingsDetailsBlock)
            {
                return "WidgetsOrderSettingsDetailsBlock";
            }

            return "";
        }
    }
}