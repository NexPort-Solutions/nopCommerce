using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    public class NexportWholesaleController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public NexportWholesaleController(
            IPermissionService permissionService,
            INexportPluginModelFactory nexportPluginModelFactory,
            ICustomerModelFactory customerModelFactory,
            IProductModelFactory productModelFactory,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            INotificationService notificationService,
            ICustomerService customerService,
            ISettingService settingService,
            ILogger logger)
        {
            _permissionService = permissionService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _customerModelFactory = customerModelFactory;
            _productModelFactory = productModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _customerService = customerService;
            _settingService = settingService;
            _logger = logger;
        }

        #endregion

        #region Actions


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetPurchaseGroupForCustomer(string groupSelected)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //save group id for customer in generic attribute so it can be saved for the order later
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


            return View("~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml", new NexportGroupListSearchModel());

        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);

            return View("~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml", nexportGroupProductListSearchModel);
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProductRedemptions(Guid groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);
            
            return View("~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml", nexportGroupProductRedemptionListSearchModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
        {

            var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid groupId)
        {

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId);

            return Json(model);
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
        {

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductCustomerListModelAsync(searchModel, groupId, productId);

            return Json(model);
        }
        #endregion
    }
}