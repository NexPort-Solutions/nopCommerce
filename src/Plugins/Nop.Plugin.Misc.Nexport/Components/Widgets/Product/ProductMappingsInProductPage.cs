using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Product;

public class ProductMappingsInProductPage : NopViewComponent
{
    private readonly Settings _settings;
    private readonly IPermissionService _permission;
    private readonly IPluginModelFactory _model;

    public ProductMappingsInProductPage(Settings settings, IPermissionService permissionService, IPluginModelFactory pluginModelFactory)
    {
        _settings = settings;
        _permission = permissionService;
        _model = pluginModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || additionalData is not ProductModel { Id: not default(int) } productModel)
        {
            return Content(string.Empty);
        }
        // todo BB TEST
        return View(_model.ProductMappingListSearchModel(productModel));
    }
}
