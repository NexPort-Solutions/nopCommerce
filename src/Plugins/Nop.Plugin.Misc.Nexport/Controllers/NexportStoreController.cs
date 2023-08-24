using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public class NexportStoreController : BasePluginController
{
    #region Fields
    private readonly IPermissionService _permissionService;
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IStoreService _storeService;

    #endregion

    #region Constructor

    public NexportStoreController(
        IPermissionService permissionService,
        INexportPluginModelFactory nexportPluginModelFactory,
        IStoreService storeService)
    {
        _permissionService = permissionService;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _storeService = storeService;
    }

    #endregion

    #region General Actions

    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return AccessDeniedView();

        return View("~/Plugins/Misc.Nexport/Views/NexportStore/List.cshtml", new NexportStoreSearchModel());
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public virtual async Task<IActionResult> List(NexportStoreSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareStoreListModel(searchModel);

        return Json(model);
    }

    [HttpGet]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public virtual async Task<IActionResult> SearchStores(string term)
    {
        var stores = (await _storeService.GetAllStoresAsync())
            .Where(store => store.Name.Contains(term))
            .Select(storeToJQueryObject);
            //.Take();
        return Json(stores);

        static JQueryObject storeToJQueryObject(Store store) => new(store.Name, store.Id.ToString());
    }
    #endregion
}
