using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models.Plugins;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Components;

[ViewComponent(Name = "WidgetsCancelPendingOrderRequestsModifiedLocaleResourcesDataTableBlock")]
public class WidgetsCancelPendingOrderRequestsModifiedLocaleResourcesDataTableBlock : NopViewComponent
{
    private readonly ISettingService _settingService;

    public WidgetsCancelPendingOrderRequestsModifiedLocaleResourcesDataTableBlock(
        ISettingService settingService)
    {
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // we will return empty content if the system name from the model from additionaldata does not have any flag return empty content 
        // the additionaldata should only be a PluginModel

        var pluginModel = (PluginModel)additionalData;

        if (pluginModel is not { FriendlyName: "Cancel pending order requests" })
            return Content("");

        var pluginSetting = await _settingService.GetSettingAsync("Plugin." + pluginModel.SystemName + ".HasModifiedLocaleResources");

        if (pluginSetting == null)
            return Content("");

        var model = new CancelPendingOrderRequestsPluginResourceListSearchModel() { FriendlyName = pluginModel.FriendlyName};

        return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/Widget/Plugin/CancelPendingOrderRequestsModifiedLocaleResources.cshtml", model);
    }
}

