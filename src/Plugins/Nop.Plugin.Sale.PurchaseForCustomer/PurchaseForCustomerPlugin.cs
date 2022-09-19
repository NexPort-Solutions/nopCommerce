using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Sale.PurchaseForCustomer
{
    public class PurchaseForCustomerPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        private readonly PurchaseForCustomerPluginService _purchaseForCustomerPluginService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public PurchaseForCustomerPlugin(
            PurchaseForCustomerPluginService purchaseForCustomerPluginService,
            WidgetSettings widgetSetting,
            ISettingService settingService)
        {
            _purchaseForCustomerPluginService = purchaseForCustomerPluginService;
            _widgetSettings = widgetSetting;
            _settingService = settingService;
        }

        public override async Task InstallAsync()
        {   
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _purchaseForCustomerPluginService.AddOrUpdateResourcesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _purchaseForCustomerPluginService.DeleteResourcesAsync();

            await base.UninstallAsync();
        }

        public bool HideInWidgetList => true;
            
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(
                new List<string>
                {
                    AdminWidgetZones.ProductDetailsButtons,
                    AdminWidgetZones.PluginDetailsBottom
                });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsButtons)
                return "ProductDetailsButtonWidget";
            if (widgetZone == AdminWidgetZones.PluginDetailsBottom)
                return "WidgetsPurchaseForCustomerModifiedLocaleResourcesDataTableBlock";

            return "";
        }
    }
}
