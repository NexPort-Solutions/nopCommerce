using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
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
        public async Task<IActionResult> SetPurchaseGroupForCustomer(Guid selectedId)
        {
            
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            // save group id for customer in generic attribute so it can be saved for the order later
            await _genericAttributeService.SaveAttributeAsync(customer, $"GroupForCustomer",
                selectedId, store.Id);

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

            try
            {
                
                var model = await _nexportPluginModelFactory.PrepareCustomerNexportGroupsModelAsync(customer.Id, pageNumber);

                var myGroupsViewLocationSetting =
                    await _settingService.GetSettingAsync("nexport.mygroups.view", (await _storeContext.GetCurrentStoreAsync()).Id, true);

                return View(myGroupsViewLocationSetting != null
                        ? myGroupsViewLocationSetting.Value
                        : "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml",model
                    );
            }
            catch (Exception ex)
            {
                var errorMsg = "Cannot display groups details.";
                await _logger.ErrorAsync(errorMsg, ex, customer);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid groupGuid, int? pageNumber)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            try
            {

                //#TODO JS - get list of products for group from model factory
                var model = await _nexportPluginModelFactory.PrepareCustomerNexportGroupProductsModelAsync(groupGuid, pageNumber);

                var myGroupProductsViewLocationSetting =
                    await _settingService.GetSettingAsync("nexport.mygroupproducts.view", (await _storeContext.GetCurrentStoreAsync()).Id, true);

                return View(myGroupProductsViewLocationSetting != null
                        ? myGroupProductsViewLocationSetting.Value
                        : "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml",model
                );
            }
            catch (Exception ex)
            {
                var errorMsg = "Cannot display group product details.";
                await _logger.ErrorAsync(errorMsg, ex, customer);
                _notificationService.ErrorNotification(errorMsg);
            }

            return new EmptyResult();
        }
        #endregion
    }
}