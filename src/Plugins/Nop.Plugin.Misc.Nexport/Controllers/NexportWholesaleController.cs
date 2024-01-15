using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    public class NexportWholesaleController : BasePluginController
    {
        #region Fields

        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly NexportService _nexportService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructor

        public NexportWholesaleController(
            INexportPluginModelFactory nexportPluginModelFactory,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            NexportService nexportService,
            IPermissionService permissionService)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _nexportService = nexportService;
            _permissionService = permissionService;
        }

        #endregion

        #region Actions


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetPurchaseGroupForCustomer(string groupSelected)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //save group for customer in generic attribute so it can be saved for the order later
            await _genericAttributeService.SaveAttributeAsync(customer, $"GroupForCustomer",
                groupSelected, store.Id);

            return Json(new
            {
                Result = true
            });
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroups(int? pageNumber)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var searchModel = new NexportGroupListSearchModel();
            searchModel.AdminView = false;
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
            ViewData["ModelForPartialView"] = searchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");

        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid? groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProductRedemptions(Guid? groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid? invoiceItemId, int? productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProduct.cshtml";
            ViewData["ModelForPartialView"] = model;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> RedeemProductForCustomer(RedeemProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            return Redirect(await _nexportService.RedeemProductForCustomer(model));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, currentCustomer);

            return Json(model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, currentCustomer);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(
            NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
            return Json(
                new { result = count }
            );
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
                await _nexportService.UnassignInvoiceItem(invoiceItem);

            return Json(
                new
                {
                    result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
                }
            );
        }

        public virtual async Task<IActionResult> SearchNexportUsers(string term)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            var nexportUsers = await _nexportService.GetNexportUsersAsync(term);

            var result = nexportUsers.Select(c => new
            {
                label = $"{c.FirstName} {c.LastName} ({c.Email})",
                nexportUserId = c.UserId
            }).ToList();

            return Json(result);
        }

        [HttpsRequirement]
        public virtual async Task<IActionResult> RedeemByEmail(int invoiceItemId, Guid nexportUserId, int productMappingId)
        {
            var model = _nexportPluginModelFactory.PrepareRedeemByEmailModel(nexportUserId, invoiceItemId,
                productMappingId);
            return View("~/Plugins/Misc.Nexport/Views/RedeemByEmail.cshtml", model);
        }

        [HttpsRequirement]
        public virtual async Task<IActionResult> RedeemAwaitingInvoiceItem(int invoiceItemId, Guid nexportUserId, int productMappingId)
        {
            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(invoiceItemId);
            if (invoiceItem != null)
                await _nexportService.RedeemAwaitingInvoiceItem(invoiceItem, nexportUserId, productMappingId);

            return Json(new
            {
                Result = true
            });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
                await _nexportService.CancelAwaitingInvoiceItem(invoiceItem);

            return Json(
                new
                {
                    result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
                }
            );
        }

        #endregion
    }

}