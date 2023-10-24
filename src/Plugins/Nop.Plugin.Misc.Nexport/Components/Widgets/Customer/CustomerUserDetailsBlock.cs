using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets;

[ViewComponent(Name = "CustomerUserDetailsBlock")]
public class CustomerUserDetailsBlock : NopViewComponent
{
    private readonly IPluginModelFactory _model;

    public CustomerUserDetailsBlock(
        IPluginModelFactory pluginModelFactory) => _model = pluginModelFactory;

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        var customerUserSettingsModel = (CustomerUserSettingsModel)additionalData;
        if (customerUserSettingsModel is null)
        {
            return Content(string.Empty);
        }

        var model = await _model.CustomerAdditionalSettingsModel();
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Setting/CustomerUserSettings.cshtml", model);
    }
}
