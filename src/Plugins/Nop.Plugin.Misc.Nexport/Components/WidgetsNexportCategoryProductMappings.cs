using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsNexportCategoryProductMappings")]
public class WidgetsNexportCategoryProductMappings : NopViewComponent
{
    private readonly NexportSettings _nexportSettings;
    private readonly IPermissionService _permissionService;

    public WidgetsNexportCategoryProductMappings(
        NexportSettings nexportSettings,
        IPermissionService permissionService)
    {
        _nexportSettings = nexportSettings;
        _permissionService = permissionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken) ||
            !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
            return Content("");

        var categoryModel = (CategoryModel)additionalData;

        var model = new NexportCategoryProductMappingListSearchModel { NopCategoryId = categoryModel.Id };

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Category/NexportCategoryProductMappings.cshtml", model);
    }
}