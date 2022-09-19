using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Archway.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Models.Plugins;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;
using Org.BouncyCastle.Math.Field;


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
        // we will return empty content if the system name from the model from additionaldata does not have any flag return empty content 
        // the additionaldata should only be a PluginModel
        var pluginModel = (PluginModel)additionalData;

        if (pluginModel is not { FriendlyName: "Archway custom registration field" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");

        if (pluginSetting == null)
            return Content("");

        // there should be no case where key's value is equal to true
        var model = new ArchwayPluginResourceListSearchModel(){ FriendlyName = pluginModel.FriendlyName };

        return View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Widget/Plugin/ArchwayModifiedLocaleResources.cshtml", model);

    }

}