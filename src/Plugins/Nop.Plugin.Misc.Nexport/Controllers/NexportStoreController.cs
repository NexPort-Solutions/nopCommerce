using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    public class NexportStoreController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;

        #endregion

        #region Constructor

        public NexportStoreController(
            IPermissionService permissionService,
            INexportPluginModelFactory nexportPluginModelFactory)
        {
            _permissionService = permissionService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
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

        #endregion
    }
}
