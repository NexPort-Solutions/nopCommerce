using System;
using System.Collections.Generic;
using System.Linq;
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
using Nop.Web.Framework.Security;
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
        private readonly INotificationService _notificationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
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
            INotificationService notificationService,
            IMessageTokenProvider messageTokenProvider,
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
            _notificationService = notificationService;
            _messageTokenProvider = messageTokenProvider;
            _workContext = workContext;
            _storeContext = storeContext;
            _logger = logger;
            _webHelper = webHelper;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestSearchModel(new PendingOrderCancellationRequestSearchModel());

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/List.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult List(PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var model = _pendingOrderCancellationRequestModelFactory
                .PreparePendingOrderCancellationRequestListModel(searchModel);

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [Route("Admin/CancelPendingOrderRequests/Edit/{requestId}")]
        public IActionResult Edit(int requestId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var returnRequest = _pendingOrderCancellationRequestService.GetCancellationRequestById(requestId);
            if (returnRequest == null)
                return RedirectToAction("List");

            var model = _pendingOrderCancellationRequestModelFactory.PreparePendingOrderCancellationRequestModel(null, returnRequest);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [Route("Admin/CancelPendingOrderRequests/Edit/{requestId}")]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [AdminAntiForgery]
        public IActionResult Edit(PendingOrderCancellationRequestModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var cancellationRequest = _pendingOrderCancellationRequestService.GetCancellationRequestById(model.Id);
            if (cancellationRequest == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                cancellationRequest = model.ToEntity(cancellationRequest);
                cancellationRequest.UtcLastModifiedDate = DateTime.UtcNow;

                _pendingOrderCancellationRequestService.UpdateCancellationRequest(cancellationRequest);

                var order = _orderService.GetOrderById(cancellationRequest.OrderId);
                if (order != null)
                {
                    if (cancellationRequest.RequestStatus == PendingOrderCancellationRequestStatus.Accepted)
                    {
                        _pendingOrderCancellationRequestService.SendCancellationRequestCustomerNotification(
                            cancellationRequest, order, order.CustomerLanguageId,
                            PluginDefaults.CANCELLATION_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);

                        try
                        {
                            _orderProcessingService.CancelOrder(order, true);
                            _customerActivityService.InsertActivity("EditOrder",
                                string.Format(_localizationService.GetResource("ActivityLog.EditOrder"),
                                    order.CustomOrderNumber), order);
                        }
                        catch (Exception ex)
                        {
                            _notificationService.ErrorNotification(ex);

                            model = _pendingOrderCancellationRequestModelFactory
                                .PreparePendingOrderCancellationRequestModel(model, cancellationRequest, true);

                            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/Edit.cshtml",
                                model);
                        }
                    }
                    else if (cancellationRequest.RequestStatus == PendingOrderCancellationRequestStatus.Rejected)
                    {
                        _pendingOrderCancellationRequestService.SendCancellationRequestCustomerNotification(
                            cancellationRequest, order, order.CustomerLanguageId,
                            PluginDefaults.CANCELLATION_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);
                    }
                }

                _customerActivityService.InsertActivity(PluginDefaults.EDIT_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                        string.Format(_localizationService.GetResource("ActivityLog.EditCancellationRequest"), cancellationRequest.Id),
                        cancellationRequest);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.CancellationRequests.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = cancellationRequest.Id }) : RedirectToAction("List");
            }

            model = _pendingOrderCancellationRequestModelFactory.PreparePendingOrderCancellationRequestModel(model, cancellationRequest, true);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/Edit.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var cancellationRequest = _pendingOrderCancellationRequestService.GetCancellationRequestById(id);
            if (cancellationRequest == null)
                return RedirectToAction("List");

            _pendingOrderCancellationRequestService.DeleteCancellationRequest(cancellationRequest);

            _customerActivityService.InsertActivity(PluginDefaults.DELETE_CANCELLATION_REQUEST_ACTIVITY_LOG_TYPE,
                string.Format(_localizationService.GetResource("ActivityLog.DeleteCancellationRequest"), cancellationRequest.Id),
                cancellationRequest);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.CancellationRequests.Deleted"));

            return RedirectToAction("List");
        }

        #region Customer Areas

        [HttpsRequirement(SslRequirement.Yes)]
        public IActionResult CancellationRequest(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (order.OrderStatus != OrderStatus.Pending)
                return RedirectToRoute("Homepage");

            var model = new SubmitCancellationRequestModel();
            model = _pendingOrderCancellationRequestModelFactory.PrepareSubmitCancellationRequestModel(model, order);

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Views/CancellationRequest.cshtml", model);
        }

        [HttpPost, ActionName("CancellationRequest")]
        [PublicAntiForgery]
        public IActionResult CancellationRequestSubmit(int orderId, SubmitCancellationRequestModel model, IFormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            if (order.OrderStatus != OrderStatus.Pending)
                return RedirectToRoute("Homepage");

            var requestReason = _pendingOrderCancellationRequestService.GetCancellationRequestReasonById(model.CancellationRequestReasonId);

            var cancellationRequest = new PendingOrderCancellationRequest
            {
                OrderId = orderId,
                CustomerId = _workContext.CurrentCustomer.Id,
                StoreId = _storeContext.CurrentStore.Id,
                CustomerComments = model.Comments,
                RequestStatus = PendingOrderCancellationRequestStatus.Received,
                ReasonForCancellation = requestReason != null ? _localizationService.GetLocalized(requestReason, x => x.Name) : "not available",
                StaffNotes = string.Empty,
                UtcCreatedDate = DateTime.UtcNow,
                UtcLastModifiedDate = DateTime.UtcNow
            };

            _pendingOrderCancellationRequestService.InsertCancellationRequest(cancellationRequest);

            _pendingOrderCancellationRequestService.SendNewCancellationRequestStoreOwnerNotification(
                cancellationRequest, order, _localizationSettings.DefaultAdminLanguageId);

            _pendingOrderCancellationRequestService.SendNewCancellationRequestCustomerNotification(
                cancellationRequest, order, order.CustomerLanguageId);

            model = _pendingOrderCancellationRequestModelFactory.PrepareSubmitCancellationRequestModel(model, order);
            model.Result = _localizationService.GetResource("CancellationRequests.Submitted");

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Views/CancellationRequest.cshtml", model);
        }

        #endregion

        public void HandleEvent(AdditionalTokensAddedEvent eventMessage)
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
                _logger.Error("Cannot add additional message tokens", ex);
            }
        }
    }
}
