using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Events;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Controllers
{
    [ResponseCache(Duration = 0, NoStore = true)]
    public class CancelPendingOrderRequestsController : BasePluginController,
        IConsumer<AdditionalTokensAddedEvent>
    {
        private readonly LocalizationSettings _localizationSettings;
        private readonly IPendingOrderCancellationRequestModelFactory _pendingOrderCancellationRequestModelFactory;
        private readonly IPendingOrderCancellationRequestService _pendingOrderCancellationRequestService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        public CancelPendingOrderRequestsController(
            LocalizationSettings localizationSettings,
            IPendingOrderCancellationRequestModelFactory pendingOrderCancellationRequestModelFactory,
            IPendingOrderCancellationRequestService pendingOrderCancellationRequestService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper)
        {
            _localizationSettings = localizationSettings;
            _pendingOrderCancellationRequestModelFactory = pendingOrderCancellationRequestModelFactory;
            _pendingOrderCancellationRequestService = pendingOrderCancellationRequestService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _logger = logger;
            _webHelper = webHelper;
        }

        #region Utilities

        protected async Task UpdateLocalesAsync(PendingOrderCancellationRequestReason reason, PendingOrderCancellationRequestReasonModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(reason,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        #endregion

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestSearchModelAsync(new PendingOrderCancellationRequestSearchModel());

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/List.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> List(PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return await AccessDeniedDataTablesJson();

            var model = _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [Route("Admin/CancelPendingOrderRequests/Edit/{requestId}")]
        public async Task<IActionResult> Edit(int requestId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var returnRequest =
                await _pendingOrderCancellationRequestService.GetCancellationRequestByIdAsync(requestId);
            if (returnRequest == null)
                return RedirectToAction("List");

            var model =
                _pendingOrderCancellationRequestModelFactory.PreparePendingOrderCancellationRequestModelAsync(null, returnRequest);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [Route("Admin/CancelPendingOrderRequests/Edit/{requestId}")]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(PendingOrderCancellationRequestModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var cancellationRequest = await _pendingOrderCancellationRequestService.GetCancellationRequestByIdAsync(model.Id);
            if (cancellationRequest == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                if (cancellationRequest.RequestStatus == PendingOrderCancellationRequestStatus.Received)
                {
                    cancellationRequest = model.ToEntity(cancellationRequest);
                    cancellationRequest.UtcLastModifiedDate = DateTime.UtcNow;

                    await _pendingOrderCancellationRequestService.UpdateCancellationRequestAsync(cancellationRequest);

                    var order = await _orderService.GetOrderByIdAsync(cancellationRequest.OrderId);
                    if (order != null)
                    {
                        if (cancellationRequest.RequestStatus == PendingOrderCancellationRequestStatus.Accepted)
                        {
                            await _pendingOrderCancellationRequestService.SendCancellationRequestCustomerNotificationAsync(
                                cancellationRequest, order, order.CustomerLanguageId,
                                PluginDefaults.CANCELLATION_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);

                            try
                            {
                                await _orderProcessingService.CancelOrderAsync(order, true);

                                await _pendingOrderCancellationRequestService.VoidCancelledOrderAsync(order);

                                await _customerActivityService.InsertActivityAsync("EditOrder",
                                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"),
                                        order.CustomOrderNumber), order);
                            }
                            catch (Exception ex)
                            {
                                await _notificationService.ErrorNotificationAsync(ex);

                                model = await _pendingOrderCancellationRequestModelFactory
                                    .PreparePendingOrderCancellationRequestModelAsync(model, cancellationRequest, true);

                                return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/Edit.cshtml",
                                    model);
                            }
                        }
                        else if (cancellationRequest.RequestStatus == PendingOrderCancellationRequestStatus.Rejected)
                        {
                            await _pendingOrderCancellationRequestService.SendCancellationRequestCustomerNotificationAsync(
                                cancellationRequest, order, order.CustomerLanguageId,
                                PluginDefaults.CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);
                        }
                    }

                    await _customerActivityService.InsertActivityAsync(PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCancellationRequest"), cancellationRequest.Id),
                            cancellationRequest);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CancellationRequests.Updated"));

                    return continueEditing ? RedirectToAction("Edit", new { id = cancellationRequest.Id }) : RedirectToAction("List");
                }

                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.CancellationRequests.CannotModified"));

                return RedirectToAction("List");
            }

            model = await _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestModelAsync(model, cancellationRequest, true);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var cancellationRequest = await _pendingOrderCancellationRequestService.GetCancellationRequestByIdAsync(id);
            if (cancellationRequest == null)
                return RedirectToAction("List");

            await _pendingOrderCancellationRequestService.DeleteCancellationRequestAsync(cancellationRequest);

            await _customerActivityService.InsertActivityAsync(PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCancellationRequest"), cancellationRequest.Id),
                cancellationRequest);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CancellationRequests.Deleted"));

            return RedirectToAction("List");
        }

        #region Customer Areas

        [HttpsRequirement]
        public async Task<IActionResult> CancellationRequest(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted || currentCustomer.Id != order.CustomerId)
                return Challenge();

            if (order.OrderStatus != OrderStatus.Pending)
                return RedirectToRoute("Homepage");

            var model = new SubmitCancellationRequestModel();
            model = await _pendingOrderCancellationRequestModelFactory.PrepareSubmitCancellationRequestModelAsync(model, order);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Views/CancellationRequest.cshtml", model);
        }

        [HttpPost, ActionName("CancellationRequest")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CancellationRequestSubmit(int orderId, SubmitCancellationRequestModel model, IFormCollection form)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted || currentCustomer.Id != order.CustomerId)
                return Challenge();

            if (order.OrderStatus != OrderStatus.Pending)
                return RedirectToRoute("Homepage");

            var requestReason =
                await _pendingOrderCancellationRequestService.GetCancellationRequestReasonByIdAsync(model.CancellationRequestReasonId);
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var cancellationRequest = new PendingOrderCancellationRequest
            {
                OrderId = orderId,
                CustomerId = currentCustomer.Id,
                StoreId = currentStore.Id,
                CustomerComments = model.Comments,
                RequestStatus = PendingOrderCancellationRequestStatus.Received,
                ReasonForCancellation = requestReason != null
                    ? await _localizationService.GetLocalizedAsync(requestReason, x => x.Name)
                    : "not available",
                StaffNotes = string.Empty,
                UtcCreatedDate = DateTime.UtcNow,
                UtcLastModifiedDate = DateTime.UtcNow
            };

            await _pendingOrderCancellationRequestService.InsertCancellationRequestAsync(cancellationRequest);

            await _pendingOrderCancellationRequestService.SendNewCancellationRequestStoreOwnerNotificationAsync(
                cancellationRequest, order, _localizationSettings.DefaultAdminLanguageId);

            await _pendingOrderCancellationRequestService.SendNewCancellationRequestCustomerNotificationAsync(
                cancellationRequest, order, order.CustomerLanguageId);

            model = await _pendingOrderCancellationRequestModelFactory.PrepareSubmitCancellationRequestModelAsync(model, order);
            model.Result = await _localizationService.GetResourceAsync("CancellationRequests.Submitted");

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Views/CancellationRequest.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> CancellationRequestReasonList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Select an appropriate panel
            SaveSelectedTabName("ordersettings-cancellation-request");

            return RedirectToAction("Order", "Setting");
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public async Task<IActionResult> CancellationRequestReasonList(PendingOrderCancellationRequestReasonSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            var model = _pendingOrderCancellationRequestModelFactory.PreparePendingOrderCancellationRequestReasonListModelAsync(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> CancellationRequestReasonCreate()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _pendingOrderCancellationRequestModelFactory.
                PreparePendingOrderCancellationRequestReasonModelAsync(new PendingOrderCancellationRequestReasonModel(), null);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/CancellationRequestReasonCreate.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CancellationRequestReasonCreate(PendingOrderCancellationRequestReasonModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var cancellationRequestReasonModel = model.ToEntity<PendingOrderCancellationRequestReason>();
                await _pendingOrderCancellationRequestService.InsertCancellationRequestReasonAsync(cancellationRequestReasonModel);

                await UpdateLocalesAsync(cancellationRequestReasonModel, model);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added"));

                return continueEditing
                    ? RedirectToAction("CancellationRequestReasonEdit", new { id = cancellationRequestReasonModel.Id })
                    : RedirectToAction("CancellationRequestReasonList");
            }

            model = await _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestReasonModelAsync(model, null);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/CancellationRequestReasonCreate.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> CancellationRequestReasonEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var cancellationRequestReason =
                await _pendingOrderCancellationRequestService.GetCancellationRequestReasonByIdAsync(id);
            if (cancellationRequestReason == null)
                return RedirectToAction("CancellationRequestReasonList");

            var model = await _pendingOrderCancellationRequestModelFactory
                    .PreparePendingOrderCancellationRequestReasonModelAsync(null, cancellationRequestReason);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/CancellationRequestReasonEdit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CancellationRequestReasonEdit(PendingOrderCancellationRequestReasonModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var cancellationRequestReason =
                await _pendingOrderCancellationRequestService.GetCancellationRequestReasonByIdAsync(model.Id);
            if (cancellationRequestReason == null)
                return RedirectToAction("CancellationRequestReasonList");

            if (ModelState.IsValid)
            {
                cancellationRequestReason = model.ToEntity(cancellationRequestReason);
                await _pendingOrderCancellationRequestService.UpdateCancellationRequestReasonAsync(cancellationRequestReason);

                await UpdateLocalesAsync(cancellationRequestReason, model);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Updated"));

                if (!continueEditing)
                    return RedirectToAction("CancellationRequestReasonList");

                return RedirectToAction("CancellationRequestReasonEdit", new { id = cancellationRequestReason.Id });
            }

            model = await _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestReasonModelAsync(model, cancellationRequestReason);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/CancellationRequest/CancellationRequestReasonEdit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> CancellationRequestReasonDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var cancellationRequestReason =
                await _pendingOrderCancellationRequestService.GetCancellationRequestReasonByIdAsync(id)
                ?? throw new ArgumentException("No cancellation request reason found with the specified id", nameof(id));

            try
            {
                await _pendingOrderCancellationRequestService.DeleteCancellationRequestReasonAsync(cancellationRequestReason);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Deleted"));

                return RedirectToAction("CancellationRequestReasonList");

            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);

                return RedirectToAction("CancellationRequestReasonEdit", new { id = cancellationRequestReason.Id });
            }
        }

        #endregion

        public async Task HandleEventAsync(AdditionalTokensAddedEvent eventMessage)
        {
            try
            {
                eventMessage.AddTokens(
                    "%Store.Name%",
                    "%Store.URL%",
                    "%Store.Email%",
                    "%Store.CompanyName%",
                    "%Store.CompanyAddress%",
                    "%Store.CompanyPhoneNumber%",
                    "%Store.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%"
                );

                eventMessage.AddTokens(
                    "%Customer.Email%",
                    "%Customer.Username%",
                    "%Customer.FullName%",
                    "%Customer.FirstName%",
                    "%Customer.LastName%",
                    "%Customer.VatNumber%",
                    "%Customer.VatNumberStatus%",
                    "%Customer.CustomAttributes%",
                    "%Customer.PasswordRecoveryURL%",
                    "%Customer.AccountActivationURL%",
                    "%Customer.EmailRevalidationURL%",
                    "%Wishlist.URLForCustomer%"
                );

                eventMessage.AddTokens(
                    "%Order.OrderNumber%",
                    "%Order.CustomerFullName%",
                    "%Order.CustomerEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingCounty%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingCounty%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.Product(s)%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForCustomer%",
                    "%Order.PickupInStore%",
                    "%Order.OrderId%"
                );

                eventMessage.AddTokens(
                    "%CancellationRequest.Id%",
                    "%CancellationRequest.OrderId%",
                    "%CancellationRequest.Reason%",
                    "%CancellationRequest.CustomerComment%",
                    "%CancellationRequest.StaffNotes%",
                    "%CancellationRequest.Status%"
                );
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot add additional message tokens", ex);
            }
        }
    }
}
