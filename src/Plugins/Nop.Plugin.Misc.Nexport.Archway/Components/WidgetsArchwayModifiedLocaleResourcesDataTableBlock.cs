using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Archway.Components;

[ViewComponent(Name = "WidgetsArchwayModifiedLocaleResourcesDataTableBlock")]
public class WidgetsArchwayModifiedLocaleResourcesDataTableBlock : NopViewComponent
{
    private readonly ISettingService _settingService;

    public WidgetsArchwayModifiedLocaleResourcesDataTableBlock(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var pluginModel = (PluginModel)additionalData;
        if (pluginModel is not { SystemName: "Misc.Nexport.Archway" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync($"Plugin.{pluginModel.SystemName}.HasModifiedLocaleResources");
        if (pluginSetting == null)
            return Content("");

        var model = new ArchwayPluginResourceListSearchModel() { FriendlyName = pluginModel.FriendlyName };

        return View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Widget/Plugin/ArchwayModifiedLocaleResources.cshtml", model);
    }
}