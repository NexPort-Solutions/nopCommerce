using Nop.Core;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets;

[ViewComponent(Name = "RegistrationFields")]
public class RegistrationFields : NopViewComponent
{
    private readonly Settings _settings;
    private readonly IPluginModelFactory _model;
    private readonly IStoreContext _storeContext;

    public RegistrationFields(
        Settings settings,
        IPluginModelFactory pluginModelFactory,
        IStoreContext storeContext)
    {
        _settings = settings;
        _model = pluginModelFactory;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object __)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return Content(string.Empty);
        }

        var model = await _model.RegistrationField.CustomerAnswers(await _storeContext.GetCurrentStoreAsync());
        return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/RegistrationFields.cshtml", model);
    }
}
