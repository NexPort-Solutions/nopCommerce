using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
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
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Constructor

        public NexportWholesaleController(
            INexportPluginModelFactory nexportPluginModelFactory,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            NexportService nexportService,
            ILogger logger,
        IPermissionService permissionService,
            LocalizationSettings localizationSettings,
            IOrderService orderService)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _nexportService = nexportService;
            _logger = logger;
            _permissionService = permissionService;
            _localizationSettings = localizationSettings;
            _orderService = orderService;
        }

        #endregion

        #region Actions


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetPurchaseGroupForCustomer(string groupSelected)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

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

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

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

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

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

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> RedeemProduct(Guid? groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, productId);
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProduct.cshtml";
            ViewData["ModelForPartialView"] = model;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> RedeemProductForCustomer(RedeemProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            var userMapping = await _nexportService.FindUserMappingByCustomerId(model.CustomerId);
            if (userMapping != null)
            {
                var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);
                if (invoiceItem != null)
                {
                    var orderInfo = await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId,
                        invoiceItem.OrderItemId);
                    if (orderInfo != null && orderInfo.Available>0)
                    {

                        var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
                        if (order != null)
                        {
                            var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);
                            if (orderItem != null)
                            {
                                if (model.AssignmentType == "Instant")
                                {
                                    if (model.SelectedProductMappingId != null)
                                    {
                                        invoiceItem.RedemptionStatus =
                                            NexportOrderInvoiceItemRedemptionStatus.Processing;
                                        invoiceItem.RedeemingUserId = userMapping.NexportUserId;
                                        await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);


                                        orderInfo.Available--;
                                        await _nexportService.UpdateWholesaleOrderInfoAsync(orderInfo);


                                        if (model.ProductMappingIdForOpenEndedProduct != null)
                                        {
                                            var productMapping =
                                                await _nexportService.GetProductMappingById(model
                                                    .SelectedProductMappingId.Value);
                                            await _genericAttributeService.SaveAttributeAsync(orderItem,
                                                $"SelectedMappingForOpenEndedProduct-{order.Id}-{orderItem.Id}",
                                                JsonConvert.SerializeObject(productMapping), order.StoreId);


                                            await _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
                                                new NexportOrderInvoiceRedemptionQueueItem
                                                {
                                                    OrderInvoiceItemId = invoiceItem.Id,
                                                    RedeemingUserId = userMapping.NexportUserId,
                                                    ProductMappingId =
                                                        model.ProductMappingIdForOpenEndedProduct.Value,
                                                    OrderItemId = invoiceItem.OrderItemId,
                                                    UtcDateCreated = DateTime.UtcNow
                                                });
                                        }
                                        else
                                        {
                                            await _nexportService.InsertNexportOrderInvoiceRedemptionQueueItem(
                                                new NexportOrderInvoiceRedemptionQueueItem
                                                {
                                                    OrderInvoiceItemId = invoiceItem.Id,
                                                    RedeemingUserId = userMapping.NexportUserId,
                                                    ProductMappingId = model.SelectedProductMappingId.Value,
                                                    OrderItemId = invoiceItem.OrderItemId,
                                                    UtcDateCreated = DateTime.UtcNow
                                                });
                                        }
                                    }
                                }
                                else
                                {

                                    var customer = await _customerService.GetCustomerByIdAsync(userMapping.NopUserId);
                                    if (customer != null)
                                    {
                                        await _nexportService.SendNewNexportManualRedemptionCustomerNotificationAsync(
                                            customer, order,
                                            _localizationSettings.DefaultAdminLanguageId);
                                        invoiceItem.RedeemingUserId = userMapping.NexportUserId;
                                        invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Awaiting;

                                        await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);

                                        orderInfo.Available--;
                                        orderInfo.Awaiting++;
                                        await _nexportService.UpdateWholesaleOrderInfoAsync(orderInfo);

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Redirect(model.returnUrl);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, currentCustomer);

            return Json(model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, currentCustomer);

            return Json(model);
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
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
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
            return Json(
                new { result = count }
            );
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
            {
                try
                {
                    var wholesaleOrderInfo =
                        await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId,
                            invoiceItem.OrderItemId);

                    if (wholesaleOrderInfo != null && wholesaleOrderInfo.Redeemed > 0)
                    {
                        invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Processing;

                        await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);


                        wholesaleOrderInfo.Redeemed--;
                        await _nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                        await _nexportService.InsertNexportOrderInvoiceResetRedemptionQueueItem(
                            new NexportOrderInvoiceResetRedemptionQueueItem
                            {
                                OrderInvoiceItemId = invoiceItem.Id,
                                UtcDateCreated = DateTime.UtcNow,
                                RetryCount = 0
                            });
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Failed to insert invoice item {invoiceItem.InvoiceItemId} into the reset redemption queue", ex);
                }
            }

            return Json(
                new
                {
                    result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
                }
            );
        }

        public virtual async Task<IActionResult> SearchCustomers(string term)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesalePurchases))
                return AccessDeniedView();

            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            var customers = await _nexportService.SearchCustomersAsync(term);

            var result = customers.Select(c => new
            {
                label = $"{c.FirstName} {c.LastName} ({c.Email})",
                customerId = c.Id
            }).ToList();

            return Json(result);
        }

        #endregion
    }
}