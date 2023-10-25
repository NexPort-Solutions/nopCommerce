using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
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
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public NexportWholesaleController(
            INexportPluginModelFactory nexportPluginModelFactory,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            NexportService nexportService,
            ILogger logger)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _nexportService = nexportService;
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

            var searchModel = new NexportGroupListSearchModel();
            searchModel.AdminView = false;
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
            ViewData["ModelForPartialView"] = searchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");

        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProductRedemptions(Guid groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> RedeemProduct(Guid groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, productId);
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
            ViewData["ModelForPartialView"] = model;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
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

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(
            NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
        {
            var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
            return Json(
                new { result = count }
            );
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
        {

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);
            //TODO @js - 
            //get invoice item by id
            //insert reset redemption queue item for the invoice item
            // return a status of "processing"
            if (invoiceItem != null)
            {
                try{

                    invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Processing;

                    await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);

                    await _nexportService.InsertNexportOrderInvoiceResetRedemptionQueueItem(
                        new NexportOrderInvoiceResetRedemptionQueueItem
                        {
                            OrderInvoiceItemId = invoiceItem.Id, UtcDateCreated = DateTime.UtcNow, RetryCount = 0
                        });
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Failed to insert invoice item {invoiceItem.InvoiceItemId} into the reset redemption queue",ex);
                }
            }

            return Json(
                new
                {
                    result = invoiceItem!=null?invoiceItem.RedemptionStatus.GetDisplayName():"",
                    //redirect = Url.RouteUrl("/")
                }
            );
        }

        // TODO @js - implement later or remove
        //[HttpPost]
        //[AutoValidateAntiforgeryToken]
        //public async Task<IActionResult> InvoiceItemCancelAwaiting(int invoiceItemId)
        //{
            
        //    return Json(
        //        new { result = true }
        //    );
        //}

        #endregion
    }
}