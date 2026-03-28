using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Models.Plugins;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsNexportModifiedLocaleResourcesDataTableBlock")]
public class WidgetsNexportModifiedLocaleResourcesDataTableBlock : NopViewComponent
{
    private readonly ISettingService _settingService;

    public WidgetsNexportModifiedLocaleResourcesDataTableBlock(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var pluginModel = (PluginModel)additionalData;

        if (pluginModel is not { SystemName: "Misc.Nexport" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync($"Plugin.{pluginModel.SystemName}.HasModifiedLocaleResources");
        if (pluginSetting == null)
            return Content("");

        var model = new NexportPluginResourceListSearchModel { FriendlyName = pluginModel.FriendlyName };

        return await ViewAsync("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Plugin/NexportModifiedLocaleResources.cshtml", model);
    }
}