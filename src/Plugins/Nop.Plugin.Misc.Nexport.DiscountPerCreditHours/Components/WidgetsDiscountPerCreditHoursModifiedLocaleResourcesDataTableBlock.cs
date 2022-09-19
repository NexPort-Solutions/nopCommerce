using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models.Plugins;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Components;

[ViewComponent(Name = "WidgetsDiscountPerCreditHoursModifiedLocaleResourcesDataTableBlock")]
public class WidgetsDiscountPerCreditHoursModifiedLocaleResourcesDataTableBlock : NopViewComponent
{
    private readonly ISettingService _settingService;

    public WidgetsDiscountPerCreditHoursModifiedLocaleResourcesDataTableBlock(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var pluginModel = (PluginModel)additionalData;

        if (pluginModel is not { FriendlyName: "Based on credit hours from Nexport product" })
            return Content("");

        var pluginSetting =
            await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");

        if (pluginSetting == null)
            return Content("");

        var model = new DiscountPerCreditHoursPluginResourceListSearchModel() { FriendlyName = pluginModel.FriendlyName };

        return View("~/Plugins/Misc.Nexport.DiscountPerCreditHours/Areas/Admin/Views/Plugin/DiscountPerCreditHoursModifiedLocaleResources.cshtml", model);
    }
}