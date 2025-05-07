using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;
using JsonSerializer = System.Text.Json.JsonSerializer;
using RedeemProductModel = Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProductModel;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportWholesaleController : BaseAdminController
{
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly IStoreService _storeService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly INexportWholesaleService _nexportWholesaleService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly NexportService _nexportService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _productService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILogger _logger;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IReturnRequestService _returnRequestService;
    private readonly ICustomNumberFormatter _customNumberFormatter;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly LocalizationSettings _localizationSettings;
    private readonly NexportSettings _nexportSettings;
    private readonly IStaticCacheManager _staticCacheManager;

    public NexportWholesaleController(
        NexportSettings nexportSettings,
        LocalizationSettings localizationSettings,
        INexportPluginModelFactory nexportPluginModelFactory,
        IPermissionService permissionService,
        IStoreService storeService,
        IOrderService orderService,
        ICustomerService customerService,
        INexportWholesaleService nexportWholesaleService,
        IWorkContext workContext, NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService productService,
        IGenericAttributeService genericAttributeService,
        IStaticCacheManager staticCacheManager,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IReturnRequestService returnRequestService,
        ICustomNumberFormatter customNumberFormatter,
        IWorkflowMessageService workflowMessageService,
        ILocalizedEntityService localizedEntityService,
        ILogger logger,
        IStoreContext storeContext,
        ICustomerActivityService customerActivityService)
    {
        _nexportSettings = nexportSettings;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _permissionService = permissionService;
        _storeService = storeService;
        _orderService = orderService;
        _customerService = customerService;
        _nexportWholesaleService = nexportWholesaleService;
        _workContext = workContext;
        _nexportService = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _productService = productService;
        _genericAttributeService = genericAttributeService;
        _staticCacheManager = staticCacheManager;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _returnRequestService = returnRequestService;
        _customNumberFormatter = customNumberFormatter;
        _workflowMessageService = workflowMessageService;
        _customerActivityService = customerActivityService;
        _localizedEntityService = localizedEntityService;
        _localizationSettings = localizationSettings;
        _storeContext = storeContext;
        _logger = logger;
    }

    #region Misc Helpers

    private async Task<bool> CheckWholesaleViewPermission()
    {
        if (await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return true;

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active)
            return false;

        var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
        if (nexportUserMapping == null)
            return false;

        var groupPermissionSearchCacheKey = new CacheKey("Misc.Nexport.SearchGroupForPermission.{0}-{1}",
            nexportUserMapping.NexportUserId.ToString(), _nexportSettings.RootOrganizationId?.ToString())
        {
            CacheTime = 30
        };

        var groupSearchResult = await _staticCacheManager.GetAsync(groupPermissionSearchCacheKey,
            async () => (await _nexportService.SearchGroupsForPermissionAsync(nexportUserMapping.NexportUserId,
                // ReSharper disable once PossibleInvalidOperationException
                _nexportSettings.RootOrganizationId.Value)));

        var hasPermission = groupSearchResult.Any();
        return hasPermission;
    }

    #endregion

    #region Bulk Purchase Operations

    [Route("Admin/Wholesale/Create")]
    public virtual async Task<IActionResult> CreateWholesaleOrder()
    {
        var permissionResult = await CheckWholesaleViewPermission();
        if (!permissionResult)
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync(null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/CreateWholesaleOrder.cshtml", model);
    }

    [Route("Admin/Wholesale/PurchasingProductsList")]
    public virtual async Task<IActionResult> WholesaleOrderPurchasingProductsList(ICollection<int> selectedIds)
    {
        var permissionResult = await CheckWholesaleViewPermission();
        if (!permissionResult)
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderPurchasingProductsListAsync(selectedIds.ToList());

        return PartialView("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesaleOrderPurchasingProducts.cshtml", model);
    }

    [Route("Admin/Wholesale/AddProductsToWholesaleOrder")]
    public virtual async Task<IActionResult> AddProductToWholesaleOrder(int storeId)
    {
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderProductSearchModel(storeId);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/AddProductToWholesaleOrderPopup.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WholesaleOrderProductList(WholesaleOrderProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderProductListModelAsync(searchModel);

        return Json(model);
    }

    [Route("Admin/Wholesale/WholesaleOrderPaymentInfo")]
    public virtual async Task<IActionResult> WholesaleOrderPaymentInfo(string paymentSystemName)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderPaymentInfoModelAsync(paymentSystemName);

        return PartialView("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesaleOrderPaymentDetails.cshtml", model);
    }

    [HttpPost]
    [FormValueRequired("placewholesaleorder", "placewholesaleorder-continue")]
    [ParameterBasedOnFormName("placewholesaleorder-continue", "continueEditing")]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleOrderModel model, IFormCollection form, bool continueEditing)
    {
        //TODO: Check permissions

        var productIdsValue = JsonSerializer.Deserialize<int[]>(form["productIds"]);
        var customer = await _workContext.GetCurrentCustomerAsync();

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store == null || store.Deleted)
        {
            ModelState.AddModelError(string.Empty, "Invalid store.");
        }

        var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, model.StoreId);
        var paymentMethod = activePlugins
            .Select(plugin => plugin.ToPluginModel<PaymentMethodModel>())
            .FirstOrDefault(p => p.SystemName == model.PaymentMethod);

        if (paymentMethod == null)
        {
            ModelState.AddModelError(string.Empty, "Selected payment method is invalid.");
        }

        //if (await _nexportService.GetOrganizationDetailsAsync(model.OrganizationId) is null)
        //{
        //    ModelState.AddModelError(nameof(model.OrganizationId), "Invalid organization.");
        //}
        //if (!model.IsRedemptionPeriodUnlimited && (model.RedeemByUtc is null || model.RedeemByUtc.Value <= DateTime.UtcNow))
        //{
        //    ModelState.AddModelError(nameof(model.RedeemByUtc), $"{nameof(model.RedeemByUtc)} must be a date in the future or {nameof(model.IsRedemptionPeriodUnlimited)} must be true.");
        //}
        //if (model.Quantity is > 100_000 or < 1)
        //{
        //    ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
        //}

        if (ModelState.IsValid)
        {
            var shoppingCartItems = new List<ShoppingCartItem>();

            foreach (var productId in productIdsValue)
            {
                var shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = ShoppingCartType.ShoppingCart,
                    StoreId = model.StoreId,
                    ProductId = productId,
                    AttributesXml = null,
                    Quantity = int.Parse(form[$"itemquantity{productId}"]),
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                };

                shoppingCartItems.Add(shoppingCartItem);
            }

            var processingPaymentRequest = new ProcessPaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                OrderGuidGeneratedOnUtc = DateTime.UtcNow,
                StoreId = model.StoreId,
                CustomerId = customer.Id,
                PaymentMethodSystemName = model.PaymentMethod
            };

            var groupModel = new NexportGroupModel
            {
                OrganizationId = model.OrganizationId,
                Name = model.OrganizationName,
                ShortName = model.OrganizationShortName
            };

            var serializedGroupModelValue = JsonSerializer.Serialize(groupModel);
            await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-PurchasingGroup", serializedGroupModelValue, model.StoreId);
            await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-FundingPoolId", model.FundingPoolId, model.StoreId);
            await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-RedeemByUtc", model.RedeemByUtc, model.StoreId);

            var placedOrderResult = await _nexportWholesaleService.PlaceWholesaleOrderAsync(processingPaymentRequest, shoppingCartItems);
            if (!placedOrderResult.Success)
                throw new InvalidOperationException("Failed to place order!");

            if (!continueEditing)
            {
                var successStr = $"New bulk purchase has been placed. Click <a href=\"{Url.Action("Edit", "Order", new { id = placedOrderResult.PlacedOrder.Id })}\">here</a> to view the order details.";
                _notificationService.SuccessNotification(successStr, false);

                return RedirectToAction("List", "Order");
            }

            return RedirectToAction("Edit", "Order", new { id = placedOrderResult.PlacedOrder.Id });
        }

        model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync(model);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/CreateWholesaleOrder.cshtml", model);
    }

    #endregion

    [Route("Admin/Wholesale/NexportGroups/Products")]
    public async Task<IActionResult> AdminNexportGroupProducts()
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync();
        searchModel.AdminView = true;

        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = searchModel;
        ViewData["AdminView"] = true;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Groups/List.cshtml");
    }

    public async Task<IActionResult> AdminNexportGroupProductRedemptions(int productId, Guid? groupId)
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);
        searchModel.AdminView = true;

        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = searchModel;
        ViewData["AdminView"] = true;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Groups/List.cshtml");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
        return Json(new { result = count });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportProductRedemptionsByFundingPoolCount(int? fundingPoolId)
    {
        var count = await _nexportService.GetAvailableNexportRedemptionsByFundingPoolCountAsync(fundingPoolId);
        return Json(new { result = count });
    }

    [Route("Admin/Wholesale/Purchases/ByFundingPools/List")]
    public async Task<IActionResult> AdminNexportWholesalePurchasesByFundingPoolsList()
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportPurchasesByFundingPoolListSearchModelAsync();

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Purchases/ByFundingPools/List.cshtml", searchModel);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportWholesalePurchasesByFundingPools(NexportPurchasesByFundingPoolListSearchModel searchModel)
    {
        var model = await _nexportPluginModelFactory.PrepareNexportPurchasesByFundingPoolListModelAsync(searchModel);

        return Json(model);
    }

    public async Task<IActionResult> AdminNexportWholesalePurchasesByFundingPoolsRedemptionList(int? fundingPoolId)
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportPurchasesByFundingPoolsRedemptionListSearchModelAsync(fundingPoolId);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Purchases/ByFundingPools/RedemptionList.cshtml", searchModel);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportWholesalePurchasesByFundingPoolsRedemptions(NexportProductRedemptionListSearchModel searchModel)
    {
        var model = await _nexportPluginModelFactory.PrepareNexportWholesalePurchasesByFundingPoolsRedemptionListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> GetMatchingUsers(NexportUserAssignmentListSearchModel searchModel)
    {
        var paged = new List<NexportUserAssignmentModel>().ToPagedList(searchModel);

        if (searchModel.TableFirstDraw)
            return Json(paged);

        var model = await _nexportPluginModelFactory.PrepareNexportUserAssignmentListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
    {
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.UnassignInvoiceItem(invoiceItem);

        return Json(new { result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "" });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
    {
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.CancelAwaitingInvoiceItem(invoiceItem);

        return Json(new { result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "" });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> RefundInvoiceItem(Guid invoiceItemId)
    {
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);
        var refundResult = false;

        if (invoiceItem != null)
            refundResult = await _nexportService.ProcessRefundingInvoiceItem(invoiceItem, false);

        return Json(new { result = refundResult });
    }

    [HttpsRequirement]
    public async Task<IActionResult> RequestRefund(Guid invoiceItemId)
    {
        var model = await _nexportPluginModelFactory.PrepareInvoiceItemRefundRequestModel(invoiceItemId);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/RequestRefund.cshtml", model);
    }

    [HttpPost, ActionName("RequestRefund")]
    [AutoValidateAntiforgeryToken]
    [FormValueRequired("submitrequestrefund")]
    public async Task<IActionResult> InvoiceItemRefundRequestSubmit(SubmitInvoiceItemRefundRequestModel model, IFormCollection form)
    {
        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/RequestRefund.cshtml", model);

        try
        {
            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);

            var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
            var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted)
                throw new Exception("Order is not available!");

            var wholesaleOrderInfo = await _nexportService.GetWholesaleOrderInfoForOrderItemAsync(invoiceItem.OrderId, invoiceItem.OrderItemId);

            if (wholesaleOrderInfo == null)
                throw new Exception("Wholesale order information is missing!");

            var requestActions = await _returnRequestService.GetAllReturnRequestActionsAsync();
            var requestReasons = await _returnRequestService.GetAllReturnRequestReasonsAsync();

            var rrr = requestReasons.FirstOrDefault(x => x.Name.Contains("No longer need"));
            var rra = requestActions.FirstOrDefault(x => x.Name.Contains("Refund"));

            var store = await _storeContext.GetCurrentStoreAsync();

            var rr = new ReturnRequest
            {
                CustomNumber = "",
                StoreId = store.Id,
                OrderItemId = orderItem.Id,
                Quantity = 1,
                CustomerId = customer.Id,
                ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                CustomerComments = model.Comments,
                StaffNotes = string.Empty,
                ReturnRequestStatus = ReturnRequestStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _returnRequestService.InsertReturnRequestAsync(rr);

            //set return request custom number
            rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
            await _customerService.UpdateCustomerAsync(customer);
            await _returnRequestService.UpdateReturnRequestAsync(rr);

            if (invoiceItem.RedemptionStatus == NexportOrderInvoiceItemRedemptionStatus.Assigned)
                wholesaleOrderInfo.Redeemed--;

            wholesaleOrderInfo.ProcessingRefund++;
            invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund;

            await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
            await _nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

            await _genericAttributeService.SaveAttributeAsync(rr, "RefundRequestInvoiceItems",
                JsonConvert.SerializeObject(new List<Guid> { model.InvoiceItemId }), rr.StoreId);

            await _nexportService.InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
            {
                CustomerId = customer.Id,
                InvoiceItemId = model.InvoiceItemId,
                Description = "Refund request had been submitted.",
                Type = NexportRedemptionAuditLogTypeEnum.RequestRefund,
                UtcDateCreated = DateTime.UtcNow
            });

            model = await _nexportPluginModelFactory.PrepareSubmitInvoiceItemRefundRequestModelAsync(model);
            model.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");

            ViewBag.RefreshPage = true;
            ViewBag.ClosePage = true;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Unable to submit new refund request", ex);
        }

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/RequestRefund.cshtml", model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> ViewRefundRequest(Guid invoiceItemId)
    {
        //var refundRequest = await _returnRequestService.SearchReturnRequestsAsync()
        return Json(null);
    }

    [Route("Admin/Wholesale/Purchases/RedemptionAuditLog/List")]
    public async Task<IActionResult> RedemptionAuditLogList(Guid invoiceItemId)
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportRedemptionAuditLogListSearchModelAsync(invoiceItemId);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Purchases/ViewRedemptionAuditLogPopup.cshtml", searchModel);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetRedemptionAuditLog(NexportRedemptionAuditLogListSearchModel searchModel)
    {
        var model = await _nexportPluginModelFactory.PrepareNexportRedemptionAuditLogListModelAsync(searchModel);

        return Json(model);
    }

    #region Redeem Product & Assign Redemption Actions

    [HttpsRequirement]
    public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid invoiceItemId, int productId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);

        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct.cshtml";
        ViewData["ModelForPartialView"] = model;
        ViewData["AdminView"] = true;
        ViewData["Title"] = "Product Assignment";

        //reset generic attributes for the redemption form
        var emailAddressAttribute = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
        if (emailAddressAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress", null);

        var sendViaEmailAttribute = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
        if (sendViaEmailAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", null);

        var selectedUserAttribute = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");
        if (selectedUserAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);

        var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
        if (firstName != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_FirstName", null);

        var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_lastName");
        if (lastName != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_LastName", null);

        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_ReturnUrl",
            Url.RouteUrl("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions") + "?groupId=" + groupId + "&productId=" + productId);

        await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId", invoiceItemId);
        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_ProductId", productId);
        await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_RedeemingProductId", null);
        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_IsOpenEnded", false);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/Groups/List.cshtml");
    }

    [HttpPost]
    public virtual async Task<IActionResult> AsnSaveCustomer(CustomerStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (model.SelectedUserId != null)
            {
                await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", false);
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", model.SelectedUserId.Value);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_EmailAddress", model.SelectedUserEmailAddress);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    //model is not valid. redisplay the form with validation message
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "customer",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_CustomerStep.cshtml", model)
                        }
                    });
                }

                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_SendViaEmail", true);
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_EmailAddress", model.Email);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_FirstName", model.FirstName);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_LastName", model.LastName);
            }

            return await GoToProductStep(customer);
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    public async Task<IActionResult> GoToProductStep(Customer customer)
    {
        var productId = await _genericAttributeService.GetAttributeAsync<int>(customer, "RedeemProductModel_ProductId");
        var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid>(customer, "RedeemProductModel_InvoiceItemId");

        var productStepModel = await _nexportPluginModelFactory.PrepareProductStepModel(productId, invoiceItemId);

        //if (productStepModel.AvailableMappings.Count < 1)
        //    throw new Exception("Error preparing step. Could not load mappings for product");

        if (productStepModel.AvailableMappings.Count == 0)
            throw new Exception("Error preparing step. Could not load mappings for product");

        if (productStepModel.AvailableMappings.Count == 1)
        {
            productStepModel.SelectedProductId = int.Parse(productStepModel.AvailableMappings.First().Value);
            return await GoToOptionStep(productStepModel, false);
        }

        return Json(new
        {
            update_section = new UpdateSectionJsonModel
            {
                name = "product",
                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductStep.cshtml", productStepModel)
            },
            goto_section = "product"
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> AsnSaveProduct(ProductStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (model.SelectedProductId == null)
                throw new Exception("There was an error retrieving the product information from the previous steps");

            var productMapping = await _nexportService.GetProductMappingByNopProductId(model.SelectedProductId.Value);
            if (productMapping == null)
                throw new Exception("There was an error retrieving the product mapping information");

            var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);
            if (product == null)
                throw new Exception("There was an error retrieving the product");

            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_RedeemingProductId", model.SelectedProductId);
            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_IsOpenEnded", model.IsOpenEnded);

            var optionStepModel = await _nexportPluginModelFactory.PrepareOptionStepModel(model.IsOpenEnded);

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "options",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductOptionStep.cshtml", optionStepModel)
                },
                goto_section = "options"
            });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> AsnProductOptions(OptionStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_StartDate", model.UtcStartDate);
            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_PurchasingForStore", model.StoreId);
            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_PurchasingGroupId", model.PurchasingGroupId);
            //await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_ExtensionAction", model.ExtensionAction);

            var confirmStepModel = await _nexportPluginModelFactory.PrepareConfirmStepModel(
                model.UtcStartDate, model.StoreId,
                model.PurchasingGroupId, form["PurchasingGroupName"],
                model.ExtensionAction);

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ConfirmStep.cshtml", confirmStepModel)
                },
                goto_section = "confirm"
            });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    public async Task<ActionResult> GoToOptionStep(ProductStepModel model, bool isOpenEnded)
    {
        var optionStepModel = await _nexportPluginModelFactory.PrepareOptionStepModel(isOpenEnded);

        return Json(new
        {
            update_section = new UpdateSectionJsonModel
            {
                name = "options",
                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductOptionStep.cshtml", optionStepModel)
            },
            goto_section = "options"
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> AsnSaveConfirm(ConfirmStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var returnUrl = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_ReturnUrl");
            var productId = await _genericAttributeService.GetAttributeAsync<int>(customer, "RedeemProductModel_ProductId");
            // The ID of the redeeming product. For open-ended products, this will be different from the ID of the original purchased product
            var redeemingProductId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_RedeemingProductId");
            var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
            var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_LastName");
            var emailAddress = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
            var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId");
            var selectedUserId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");
            var utcStartDate = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer, "RedeemProductModel_StartDate");
            var storeId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_PurchasingForStore");
            var purchasingGroupId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_PurchasingGroupId");
            var openEndedProduct = await _genericAttributeService.GetAttributeAsync<bool>(customer, "RedeemProductModel_IsOpenEnded");

            if (invoiceItemId == null)
                throw new Exception("Error retrieving invoice item id for transaction.");

            int? targetUserId = null;
            if (selectedUserId != null)
            {
                var selectedUser = await _nexportService.FindUserMappingByNexportUserId(selectedUserId.Value);
                if (selectedUser != null)
                {
                    targetUserId = selectedUser.NopUserId;
                }
            }

            // Add audit log for redemption assignment
            await _nexportService.InsertNexportRedemptionAuditLogAsync(new NexportRedemptionAuditLog
            {
                InvoiceItemId = invoiceItemId.Value,
                Description = $"The invoice item has been scheduled to be assigned and redeemed for customer [{firstName} {lastName}]",
                CustomerId = customer.Id,
                TargetedCustomerId = targetUserId,
                Type = NexportRedemptionAuditLogTypeEnum.AssignRedemption,
                UtcDateCreated = DateTime.UtcNow
            });

            var success = await _nexportService.RedeemProductForCustomer(
                new RedeemProductModel
                {
                    ReturnUrl = returnUrl ?? "",
                    ProductId = productId,
                    RedeemingProductId = redeemingProductId,
                    AssignmentType = sendViaEmail != null && sendViaEmail.Value ? "Email" : "Instant",
                    Email = emailAddress,
                    FirstName = firstName,
                    LastName = lastName,
                    InvoiceItemId = invoiceItemId.Value,
                    UserId = selectedUserId,
                    UtcStartDate = utcStartDate,
                    StoreId = storeId,
                    PurchasingGroupId = purchasingGroupId,
                    IsOpenEnded = openEndedProduct,
                    ExtensionOption = (int?)model.ExtensionOption
                });

            return Json(new { success });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    #endregion

    #region Unassign Redemption Actions

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequests/List")]
    public async Task<IActionResult> UnassignmentRequestsList()
    {
        var model = await _nexportPluginModelFactory
            .PrepareRedemptionUnassignmentRequestSearchModelAsync(new NexportRedemptionUnassignmentRequestListSearchModel());

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/List.cshtml", model);
    }

    [HttpsRequirement]
    [HttpPost]
    [Route("Admin/NexportWholesale/UnassignmentRequests/List")]
    public async Task<IActionResult> UnassignmentRequestsList(NexportRedemptionUnassignmentRequestListSearchModel searchModel)
    {
        var model = await _nexportPluginModelFactory.PrepareNexportRedemptionUnassignmentRequestListModel(searchModel);

        return Json(model);
    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequests/Edit/{requestId}")]
    public async Task<IActionResult> EditUnassignmentRequest(int requestId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var returnRequest = await _nexportService.GetNexportRedemptionUnassignmentRequestByIdAsync(requestId);
        if (returnRequest == null)
            return RedirectToAction("UnassignmentRequestsList");

        var model =
            await _nexportPluginModelFactory.PrepareRedemptionUnassignmentRequestModelAsync(null, returnRequest);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/Edit.cshtml", model);
    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequests/Edit/{requestId}")]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Edit(NexportRedemptionUnassignmentRequestModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var unassignmentRequest = await _nexportService.GetNexportRedemptionUnassignmentRequestByIdAsync(model.Id);
        if (unassignmentRequest == null)
            return RedirectToAction("UnassignmentRequestsList");

        if (ModelState.IsValid)
        {
            if (unassignmentRequest.RequestStatus == NexportRedemptionUnassignmentRequestStatus.Received)
            {
                unassignmentRequest = model.ToEntity(unassignmentRequest);
                unassignmentRequest.UtcLastModifiedDate = DateTime.UtcNow;

                await _nexportService.UpdateNexportRedemptionUnassignmentRequestAsync(unassignmentRequest);

                var invoiceItem =
                    await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(unassignmentRequest.InvoiceItemId);
                if (invoiceItem != null)
                {
                    if (unassignmentRequest.RequestStatus == NexportRedemptionUnassignmentRequestStatus.Accepted)
                    {
                        //notify accepted
                        await _nexportService.SendRedemptionUnassignmentRequestCustomerNotificationAsync(
                            unassignmentRequest, invoiceItem,
                            NexportDefaults.REDEMPTION_UNASSIGNMENT_REQUEST_ACCEPTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);

                        try
                        {
                            await _nexportService.UnassignInvoiceItem(invoiceItem);
                        }
                        catch (Exception ex)
                        {
                            await _notificationService.ErrorNotificationAsync(ex);

                            model = await _nexportPluginModelFactory
                                .PrepareRedemptionUnassignmentRequestModelAsync(model, unassignmentRequest, true);

                            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/Edit.cshtml", model);
                        }
                    }
                    else if (unassignmentRequest.RequestStatus == NexportRedemptionUnassignmentRequestStatus.Rejected)
                    {
                        //notify rejected
                        await _nexportService.SendRedemptionUnassignmentRequestCustomerNotificationAsync(
                            unassignmentRequest, invoiceItem,
                            NexportDefaults.REDEMPTION_UNASSIGNMENT_REQUEST_REJECTED_CUSTOMER_NOTIFICATION_MESSAGE_TEMPLATE);
                    }
                }

                await _customerActivityService.InsertActivityAsync(NexportDefaults.EDIT_UNASSIGNMENT_REQUEST_ACTIVITY_LOG_TYPE,
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditUnassignmentRequest"), unassignmentRequest.Id),
                        unassignmentRequest);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("RedemptionUnassignmentRequests.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = unassignmentRequest.Id }) : RedirectToAction("UnassignmentRequestsList");
            }

            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.CancellationRequests.CannotModified"));

            return RedirectToAction("UnassignmentRequestsList");
        }

        model = await _nexportPluginModelFactory
            .PrepareRedemptionUnassignmentRequestModelAsync(model, unassignmentRequest, true);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/Edit.cshtml", model);
    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/List")]
    public async Task<IActionResult> UnassignmentRequestReasonsList()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/_RedemptionUnassignmentRequestReasons.cshtml", new NexportRedemptionUnassignmentRequestReasonSearchModel());

    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/List")]
    [HttpPost]
    public async Task<IActionResult> UnassignmentRequestReasonsList(NexportRedemptionUnassignmentRequestReasonSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareRedemptionUnassignmentRequestReasonListModelAsync(searchModel);

        return Json(model);
    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/Create")]
    public async Task<IActionResult> UnassignmentRequestReasonCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.
            PrepareRedemptionUnassignmentRequestReasonModelAsync(new NexportRedemptionUnassignmentRequestReasonModel(), null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/RedemptionUnassignmentRequestReasonCreate.cshtml", model);
    }

    [HttpsRequirement]
    [AutoValidateAntiforgeryToken]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/Create")]
    public async Task<IActionResult> UnassignmentRequestReasonCreate(NexportRedemptionUnassignmentRequestReasonModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var unassignmentRequestReasonModel = model.ToEntity<NexportRedemptionUnassignmentRequestReason>();
            await _nexportService.InsertNexportRedemptionUnassignmentRequestReasonAsync(unassignmentRequestReasonModel);

            await UpdateLocalesAsync(unassignmentRequestReasonModel, model);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.CancellationRequestReasons.Added"));

            return continueEditing
                ? RedirectToAction("unassignmentRequestReasonEdit", new { id = unassignmentRequestReasonModel.Id })
                : RedirectToAction("unassignmentRequestReasonsList");
        }

        model = await _nexportPluginModelFactory
            .PrepareRedemptionUnassignmentRequestReasonModelAsync(model, null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/RedemptionUnassignmentRequestReasonCreate.cshtml", model);
    }

    [HttpsRequirement]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/Edit/{reasonId}")]
    public async Task<IActionResult> UnassignmentRequestReasonEdit(int reasonId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var unassignmentRequestReason =
            await _nexportService.GetNexportRedemptionUnassignmentRequestReasonByIdAsync(reasonId);
        if (unassignmentRequestReason == null)
            return RedirectToAction("UnassignmentRequestReasonsList");

        var model = await _nexportPluginModelFactory
                    .PrepareRedemptionUnassignmentRequestReasonModelAsync(null, unassignmentRequestReason);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/RedemptionUnassignmentRequestReasonEdit.cshtml", model);
    }

    [HttpsRequirement]
    [AutoValidateAntiforgeryToken]
    [Route("Admin/NexportWholesale/UnassignmentRequestReasons/Edit/{reasonId}")]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public async Task<IActionResult> UnassignmentRequestReasonEdit(NexportRedemptionUnassignmentRequestReasonModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var unassignmentRequestReason =
            await _nexportService.GetNexportRedemptionUnassignmentRequestReasonByIdAsync(model.Id);
        if (unassignmentRequestReason == null)
            return RedirectToAction("UnassignmentRequestReasonsList");

        if (ModelState.IsValid)
        {
            unassignmentRequestReason = model.ToEntity(unassignmentRequestReason);
            await _nexportService.UpdateNexportRedemptionUnassignmentRequestReasonAsync(unassignmentRequestReason);

            await UpdateLocalesAsync(unassignmentRequestReason, model);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("RedemptionUnassignmentRequestReasons.Updated"));

            if (!continueEditing)
                return RedirectToAction("UnassignmentRequestReasonsList");

            return RedirectToAction("UnassignmentRequestReasonEdit", new { id = unassignmentRequestReason.Id });
        }

        model = await _nexportPluginModelFactory
            .PrepareRedemptionUnassignmentRequestReasonModelAsync(model, unassignmentRequestReason);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/UnassignmentRequests/RedemptionUnassignmentRequestReasonEdit.cshtml", model);
    }

    [HttpsRequirement]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> UnassignmentRequestReasonDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AccessDeniedView();

        var unassignmentRequestReason =
            await _nexportService.GetNexportRedemptionUnassignmentRequestReasonByIdAsync(id)
            ?? throw new ArgumentException("No cancellation request reason found with the specified id", nameof(id));

        try
        {
            await _nexportService.DeleteUnassignmentRequestReasonAsync(unassignmentRequestReason);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("RedemptionUnassignmentRequestReasons.Deleted"));

            return RedirectToAction("UnassignmentRequestReasonsList");

        }
        catch (Exception ex)
        {
            await _notificationService.ErrorNotificationAsync(ex);

            return RedirectToAction("UnassignmentRequestReasonEdit", new { id = unassignmentRequestReason.Id });
        }
    }

    [HttpsRequirement]
    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var unassignmentRequest = await _nexportService.GetNexportRedemptionUnassignmentRequestByIdAsync(id);
        if (unassignmentRequest == null)
            return RedirectToAction("UnassignmentRequestsList");

        await _nexportService.DeleteNexportRedemptionUnassignmentRequestAsync(unassignmentRequest);

        await _customerActivityService.InsertActivityAsync(NexportDefaults.DELETE_UNASSIGNMENT_REQUEST_ACTIVITY_LOG_TYPE,
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteUnassignmentRequest"), unassignmentRequest.Id),
            unassignmentRequest);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("RedemptionUnassignmentRequests.Deleted"));

        return RedirectToAction("UnassignmentRequestsList");
    }

    protected async Task UpdateLocalesAsync(NexportRedemptionUnassignmentRequestReason reason, NexportRedemptionUnassignmentRequestReasonModel model)
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

    #region Funding Pool Actions

    [HttpGet]
    [Route("Admin/Wholesale/FundingPool/List")]
    public async Task<IActionResult> ListFundingPools()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolSearchModelAsync(new NexportFundingPoolSearchModel());

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/List.cshtml", model);
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    [Route("Admin/Wholesale/FundingPool/List")]
    public async Task<IActionResult> ListFundingPools(NexportFundingPoolSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolListModelAsync(searchModel);

        return Json(model);
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [Route("Admin/Wholesale/FundingPool/Create")]
    public async Task<IActionResult> CreateFundingPool()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(new NexportFundingPoolModel(), null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Create.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [AutoValidateAntiforgeryToken]
    [Route("Admin/Wholesale/FundingPool/Create")]
    public async Task<IActionResult> CreateFundingPool(NexportFundingPoolModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var fundingPool = model.ToEntity<NexportFundingPool>();
            fundingPool.UtcDateCreated = DateTime.UtcNow;

            await _nexportWholesaleService.InsertFundingPool(fundingPool);

            _notificationService.SuccessNotification("A new funding pool has been added.");

            if (!continueEditing)
                return RedirectToAction("ListFundingPools", "NexportWholesale");

            return RedirectToAction("EditFundingPool", "NexportWholesale", new { id = fundingPool.Id });
        }

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Create.cshtml", model);
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [Route("Admin/Wholesale/FundingPool/Edit/{id}")]
    public async Task<IActionResult> EditFundingPool(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(null, fundingPool);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Edit.cshtml", model);
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [Route("Admin/Wholesale/FundingPool/Edit/{id}")]
    public virtual async Task<IActionResult> EditFundingPool(NexportFundingPoolModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(model.Id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        if (ModelState.IsValid)
        {
            fundingPool = model.ToEntity(fundingPool);
            await _nexportWholesaleService.UpdateFundingPool(fundingPool);

            _notificationService.SuccessNotification("Successfully update funding pool");

            if (!continueEditing)
                return RedirectToAction("ListFundingPools", "NexportWholesale");

            return RedirectToAction("EditFundingPool", "NexportWholesale", new { id = fundingPool.Id });
        }

        model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(model, fundingPool);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Edit.cshtml", model);
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    [Route("Admin/Wholesale/FundingPool/Delete/{id}")]
    public virtual async Task<IActionResult> DeleteFundingPool(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        await _nexportWholesaleService.DeleteFundingPool(fundingPool);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.FundingPool.Deleted"));

        return RedirectToAction("ListFundingPools", "NexportWholesale");
    }

    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedFundingPools(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _nexportWholesaleService.DeleteFundingPools(await _nexportWholesaleService.GetFundingPoolByIds(selectedIds.ToArray()));

        return Json(new { Result = true });
    }

    #endregion
}