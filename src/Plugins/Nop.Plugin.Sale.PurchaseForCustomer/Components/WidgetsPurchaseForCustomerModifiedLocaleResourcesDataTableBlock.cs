using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Sale.PurchaseForCustomer.Models.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Components;

[ViewComponent(Name = "WidgetsPurchaseForCustomerModifiedLocaleResourcesDataTableBlock")]
public class WidgetsPurchaseForCustomerModifiedLocaleResourcesDataTableBlock : NopViewComponent
{
    private readonly ISettingService _settingService;

    public WidgetsPurchaseForCustomerModifiedLocaleResourcesDataTableBlock(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // we will return empty content if the system name from the model from additionaldata does not have any flag return empty content 
        // the additionaldata should only be a PluginModel

        var pluginModel = (PluginModel)additionalData;

        if (pluginModel is not { FriendlyName: "Purchase for customer" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");

        if (pluginSetting == null)
            return Content("");

        var model = new PurchaseForCustomerPluginResourceListSearchModel() { FriendlyName = pluginModel.FriendlyName };
        
        return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/Widget/Plugin/PurchaseForCustomerModifiedLocaleResources.cshtml", model);
    }
}