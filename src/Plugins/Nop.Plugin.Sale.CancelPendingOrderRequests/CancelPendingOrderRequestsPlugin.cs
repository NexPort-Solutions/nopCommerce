using System;
using System.Collections.Generic;
using System.Linq;
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
using Nop.Plugin.Sale.CancelPendingOrderRequests.Data;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests
{
    public class CancelPendingOrderRequestsPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly CancelPendingOrderRequestsObjectContext _cancelPendingOrderRequestsObjectContext;
        private readonly CancelPendingOrderRequestsPluginService _cancelCancelPendingOrderRequestsPluginService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public CancelPendingOrderRequestsPlugin(
            CancelPendingOrderRequestsObjectContext cancelPendingOrderRequestsObjectContext,
            CancelPendingOrderRequestsPluginService cancelCancelPendingOrderRequestsPluginService,
            WidgetSettings widgetSetting,
            ISettingService settingService)
        {
            _cancelPendingOrderRequestsObjectContext = cancelPendingOrderRequestsObjectContext;
            _cancelCancelPendingOrderRequestsPluginService = cancelCancelPendingOrderRequestsPluginService;
            _widgetSettings = widgetSetting;
            _settingService = settingService;
        }

        public void ManageSiteMap(SiteMapNode rootNode)
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

        public override void Install()
        {
            _cancelPendingOrderRequestsObjectContext.Install();

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _cancelCancelPendingOrderRequestsPluginService.AddActivityLogTypes();
            _cancelCancelPendingOrderRequestsPluginService.AddMessageTemplates();
            _cancelCancelPendingOrderRequestsPluginService.AddOrUpdateResources();

            base.Install();
        }

        public override void Uninstall()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _cancelCancelPendingOrderRequestsPluginService.DeleteMessageTemplates();
            _cancelCancelPendingOrderRequestsPluginService.DeleteActivityLogTypes();
            _cancelCancelPendingOrderRequestsPluginService.DeleteResources();

            try
            {
                var migrationServiceProvider = PluginStartup.CreateFluentMigratorRunnerService();
                using (var serviceScope = migrationServiceProvider.CreateScope())
                {
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
            }
            catch (Exception)
            {
                // Ignore
            }

            var versionSetting = _settingService.GetSetting(PluginDefaults.ASSEMBLY_VERSION_KEY);
            if (versionSetting != null)
            {
                _settingService.DeleteSetting(versionSetting);
            }

            _cancelPendingOrderRequestsObjectContext.Uninstall();

            base.Uninstall();
        }

        public bool HideInWidgetList => true;

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                PublicWidgetZones.OrderDetailsPageOverview
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.OrderDetailsPageOverview)
            {
                return "WidgetsOrderDetailsPageOverview";
            }

            return "";
        }
    }
}