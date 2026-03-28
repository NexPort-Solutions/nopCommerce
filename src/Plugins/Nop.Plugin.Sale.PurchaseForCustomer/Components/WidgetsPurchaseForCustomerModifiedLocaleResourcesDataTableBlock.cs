using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;
using Nop.Services.Configuration;
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
        var pluginModel = (PluginModel)additionalData;
        if (pluginModel is not { SystemName: "Sale.PurchaseForCustomer" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");
        if (pluginSetting == null)
            return Content("");

        var model = new PurchaseForCustomerPluginResourceListSearchModel { FriendlyName = pluginModel.FriendlyName };

        return await ViewAsync("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/Widget/Plugin/PurchaseForCustomerModifiedLocaleResources.cshtml", model);
    }
}