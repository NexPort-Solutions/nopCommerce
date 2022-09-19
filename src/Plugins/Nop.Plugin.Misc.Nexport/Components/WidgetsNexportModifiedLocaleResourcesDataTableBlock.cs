using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.Nexport.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
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

        if (pluginModel is not { FriendlyName: "Nexport" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");
        
        if (pluginSetting == null)
            return Content("");

        // there should be no case where key's value is equal to true
        var model = new NexportPluginResourceListSearchModel(){ FriendlyName = pluginModel.FriendlyName };

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Plugin/NexportModifiedLocaleResources.cshtml", model);
    }

}