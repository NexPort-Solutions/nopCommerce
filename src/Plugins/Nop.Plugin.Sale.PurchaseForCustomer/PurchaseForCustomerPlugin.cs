using System.Collections.Generic;
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

        public override void Install()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _purchaseForCustomerPluginService.AddOrUpdateResources();

            base.Install();
        }

        public override void Uninstall()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _purchaseForCustomerPluginService.DeleteResources();

            base.Uninstall();
        }

        public bool HideInWidgetList => true;

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                AdminWidgetZones.ProductDetailsButtons
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsButtons)
                return "ProductDetailsButtonWidget";

            return "";
        }
    }
}
