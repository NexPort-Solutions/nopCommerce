using System.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using NexportApi.Client;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Controllers;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using System.Globalization;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
public class IntegrationController : BaseAdminController
{
    #region Fields
    private readonly Settings _settings;
    private readonly ICustomerActivityService _customerActivity;
    private readonly ICustomerService _customer;
    private readonly IInvoiceService _invoiceService;
    private readonly INexportService _nexportService;
    private readonly IOrderService _order;
    private readonly Factories.IPluginModelFactory _model;
    private readonly IProductMappingService _productMapping;
    private readonly IProductService _product;
    private readonly IRegistrationFieldService _registrationField;
    private readonly ISettingService _setting;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _store;
    private readonly IUserMappingService _userMapping;
    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permission;
    private readonly ILocalizationService _localization;
    private readonly INotificationService _notification;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly ILogger _logger;
    private readonly IUserService _userService;
    private readonly IOrganizationService _organization;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly IUserService _user;
    private readonly ISignInService _signIn;
    #endregion Fields
    #region Constructor
    public IntegrationController(
        Settings settings,
        INexportService nexport,
        Factories.IPluginModelFactory pluginModelFactory,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICustomerActivityService customerActivity,
        IProductService product,
        IStoreService store,
        IOrderService order,
        ICustomerService customer,
        ISettingService setting,
        IPermissionService permission,
        ILocalizationService localization,
        INotificationService notification,
        IGenericAttributeService genericAttribute,
        ILogger logger,
        IUserMappingService userMapping,
        IProductMappingService productMapping,
        IInvoiceService invoice,
        IRegistrationFieldService registrationField,
        IUserService userService,
        IOrganizationService organization,
        ISupplementalInfoService supplementalInfo,
        IUserService user,
        ISignInService signIn)
    {
        _model = pluginModelFactory;
        _workContext = workContext;
        _storeContext = storeContext;
        _customerActivity = customerActivity;
        _product = product;
        _store = store;
        _order = order;
        _customer = customer;
        _settings = settings;
        _nexportService = nexport;
        _setting = setting;
        _permission = permission;
        _localization = localization;
        _notification = notification;
        _genericAttribute = genericAttribute;
        _logger = logger;
        _userMapping = userMapping;
        _productMapping = productMapping;
        _invoiceService = invoice;
        _registrationField = registrationField;
        _userService = userService;
        _organization = organization;
        _supplementalInfo = supplementalInfo;
        _user = user;
        _signIn = signIn;
    }
    #endregion Constructor
    #region General Actions
    [HttpGet]
    public async Task<IActionResult> SearchDirectory(string searchTerm, int? page = null)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping))
        {
            return AccessDeniedView();
        }
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await Configure();
        }
        try
        {
            var result = _nexportService.SearchDirectory(searchTerm, page);
            return new JsonResult(result);
        }
        catch (Exception exception)
        {
            var errorMessage = "Cannot search the NexPort directory.";
            if (exception is ApiException)
            {
                errorMessage += $" ({exception.Message})";
            }
            await _logger.ErrorAsync(errorMessage, exception);
            return BadRequest(errorMessage);
        }
    }
    #endregion General Actions
    #region Plugin Configuration Actions
    [ImportModelState]
    [HttpGet]
    public async Task<IActionResult> Configure()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }
        return View();
    }

    [HttpPost]
    [ExportModelState]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Configure));
        }
        try
        {
            await _nexportService.GenerateNewTokenAsync(model.Url, model.Username, model.Password, model.TokenNeverExpired, model.CustomTokenExpirationDate);
        }
        catch (ApiException exception)
        {
            var errorMessage = $"Cannot generate new NexPort authentication token! ({exception.Message})";
            await _logger.ErrorAsync(errorMessage, exception);
            _notification.ErrorNotification(errorMessage);
        }
        return RedirectToAction(nameof(Configure));
    }

    [HttpPost]
    [ActionName("Configure")]
    [FormValueRequired("setserverurl")]
    [ExportModelState]
    public async Task<IActionResult> SetServerUrl(ConfigurationModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Configure));
        }

        try
        {
            if (model.Url is not string url || !url.IsValidUrl())
            {
                return RedirectToAction(nameof(Configure));
            }

            _settings.Url = url;
            await _setting.SaveSettingAsync(_settings);
            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Plugins.Saved"));
            return RedirectToAction(nameof(Configure));
        }
        catch (Exception exception)
        {
            const string message = "Cannot set the server URL!";
            await _logger.ErrorAsync(message, exception);
            _notification.ErrorNotification(message);
        }
        return RedirectToAction(nameof(Configure));
    }

    [HttpPost]
    [ActionName("Configure")]
    [FormValueRequired("setrootorganizationid")]
    [ExportModelState]
    public async Task<IActionResult> SetRootOrganization(ConfigurationModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Configure));
        }

        try
        {
            _settings.RootOrganizationId = model.RootOrganizationId;
            await _setting.SaveSettingAsync(_settings);
            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Plugins.Saved"));
        }
        catch (Exception exception)
        {
            const string message = "Cannot set the root organization!";
            await _logger.ErrorAsync(message, exception);
            _notification.ErrorNotification(message);
        }
        return RedirectToAction(nameof(Configure));
    }

    [HttpPost]
    [ActionName("Configure")]
    [FormValueRequired("setmerchantaccountid")]
    [ExportModelState]
    public async Task<IActionResult> SetMerchantAccount(ConfigurationModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
        {
            return AccessDeniedView();
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Configure));
        }

        try
        {
            _settings.MerchantAccountId = model.MerchantAccountId;
            await _setting.SaveSettingAsync(_settings);
            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Plugins.Saved"));
        }
        catch (Exception exception)
        {
            const string message = "Cannot set the merchant account!";
            await _logger.ErrorAsync(message, exception);
            _notification.ErrorNotification(message);
        }
        return RedirectToAction(nameof(Configure));
    }
    #endregion Plugin Configuration Actions

    #region Order Management Actions
    [HttpPost]
    public async Task<IActionResult> GetOrderInvoiceItems(InvoiceItemSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageOrderInvoice))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.OrderInvoiceItemListModel(model, true));
    }

    [HttpPost]
    public async Task<IActionResult> ModifyEnrollment(int id, int? action)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageOrderInvoice))
        {
            return AccessDeniedView();
        }
        if (await _invoiceService.FindOrderInvoiceItemById(id) is not OrderInvoiceItem orderInvoiceItem
            || await _order.GetOrderByIdAsync(orderInvoiceItem.OrderId) is not { CustomerId: var orderCustomerId }
            || await _order.GetOrderItemByIdAsync(orderInvoiceItem.OrderItemId) is not { ProductId: var productId }
            || await _customer.GetCustomerByIdAsync(orderCustomerId) is not { Email: var customerEmail, Id: var customerId }
            || await _productMapping.GetByNopProductId(productId) is not { Id: var productMappingId }
            || await _userMapping.FindByCustomerId(customerId) is not { UserId: var userMappingUserId })
        {
            return new NullJsonResult();
        }
        var orderInvoiceRedemptionQueueItem = new OrderInvoiceRedemptionQueueItem
        {
            OrderInvoiceItemId = orderInvoiceItem.Id,
            RedeemingUserId = userMappingUserId,
            ProductMappingId = productMappingId,
            OrderItemId = orderInvoiceItem.OrderItemId,
            UtcDateCreated = DateTime.UtcNow,
            ManualApprovalAction = action,
            RetryCount = 0,
        };
        await _invoiceService.InsertOrderInvoiceRedemptionQueueItem(orderInvoiceRedemptionQueueItem);
        var result = new
        {
            success = true, // TODO
            message = $"NexPort invoice {orderInvoiceItem.InvoiceItemId} redemption for customer {customerEmail} [{userMappingUserId}] has been scheduled. "
                + "Please check back later for new order status.",
        };
        return Json(result);
    }
    #endregion Order Management Actions

    #region Customer Actions
    [HttpsRequirement]
    [HttpGet]
    public IActionResult ViewOrderRedemption(OrderInvoiceItem model) => View("~/Plugins/Misc.Nexport/Views/ViewOrder.cshtml", model);

    [HttpsRequirement]
    [HttpGet]
    public async Task<IActionResult> ViewTraining()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        try
        {
            var model = await _model.TrainingListModel(customer);
            var myTrainingViewLocationSetting = await _setting.GetSettingAsync("nexport.mytraining.view", (await _storeContext.GetCurrentStoreAsync()).Id, true);
            var viewName = myTrainingViewLocationSetting?.Value ?? "~/Plugins/Misc.Nexport/Views/Trainings.cshtml";
            return View(viewName, model);
        }
        catch (Exception exception)
        {
            const string errorMessage = "Cannot display training details.";
            await _logger.ErrorAsync(errorMessage, exception, customer);
            _notification.ErrorNotification(errorMessage);
        }
        return new EmptyResult();
    }
#endregion Customer Actions
    #region Redeeming Product Actions
    [HttpPost]
    public async Task<IActionResult> RedeemOrderInvoiceItem(int orderItemInvoiceId, Guid? redeemingUserId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        try
        {
            redeemingUserId ??= (await _userMapping.FindByCustomerId(customer.Id))?.UserId;
            if (redeemingUserId is { } userId
                && await _invoiceService.FindOrderInvoiceItemById(orderItemInvoiceId) is { OrderId: var orderId, InvoiceItemId: var invoiceItemId } orderInvoiceItem)
            {
                var order = await _order.GetOrderByIdAsync(orderId);
                try
                {
                    await _invoiceService.RedeemInvoiceItemAsync(orderInvoiceItem, userId);
                    var orderNote = new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"NexPort invoice item {invoiceItemId} has been redeemed for user {userId}",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    await _order.InsertOrderNoteAsync(orderNote);
                }
                catch (Exception exception)
                {
                    var orderNote = new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"NexPort invoice item {invoiceItemId} cannot be redeemed for user {userId}",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    await _order.InsertOrderNoteAsync(orderNote);
                    var errorMessage = string.Format(CultureInfo.InvariantCulture, await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedeemForUser"), invoiceItemId, userId);
                    await _logger.ErrorAsync(errorMessage, exception, customer);
                    _notification.ErrorNotification(errorMessage);
                    return new NullJsonResult();
                }
                return Json(orderInvoiceItem);
            }
        }
        catch (Exception exception)
        {
            var errorMessage = string.Format(CultureInfo.InvariantCulture, await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.RedemptionProcessFailure"), orderItemInvoiceId);
            await _logger.ErrorAsync(errorMessage, exception);
            _notification.ErrorNotification(errorMessage);
        }
        return new NullJsonResult();
    }

    [HttpGet]
    public async Task<IActionResult> GoToNexport(int orderInvoiceItemId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }

        if (orderInvoiceItemId < 1)
        {
            return Content(string.Empty);
        }

        if (await _invoiceService.FindOrderInvoiceItemById(orderInvoiceItemId) is not { } orderInvoiceItem)
        {
            return BadRequest("Order invoice not found.");
        }

        if (orderInvoiceItem.UtcDateRedemption is null)
        {
            return BadRequest("Order invoice has not been redeemed. Unable to access Nexport.");
        }

        dynamic result = new ExpandoObject();
        try
        {
            result.RedirectUrl = await _invoiceService.SignInAsync(orderInvoiceItem);
        }
        catch (Exception exception)
        {
            var errorMessage = await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
            await _logger.ErrorAsync(errorMessage, exception, customer);
            result.Error = errorMessage;
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GoToClassroom(Guid enrollmentId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        dynamic result = new ExpandoObject();
        try
        {
            result.RedirectUrl = await _signIn.SignInClassroomAsync(enrollmentId);
        }
        catch (Exception exception)
        {
            var errorMessage = await _localization.GetResourceAsync("Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport");
            await _logger.ErrorAsync(errorMessage, exception, customer);
            result.Error = errorMessage;
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GoToOrg(Guid orgId, Guid userId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        var result = await Helper(() => _signIn.SignInAsync(orgId, userId), customer, "Plugins.Misc.Nexport.Errors.FailedToRedirectToNexport") switch
        {
            Err<string> { Error: var error } => (object?)new { error },
            Ok<string> { Okay: var redirectResult } => (object?)new { redirectResult },
            _ => Absurd()
        };
        return Json(result);
    }
    #endregion Redeeming Product Actions

    private async Task<Result<T, string>> Helper<T>(Func<Task<T?>> f, Customer customer, string resource)
        where T : class
    {
        try
        {
            return Okay(await f());
        }
        catch (Exception exception)
        {
            var errorMessage = await _localization.GetResourceAsync(resource);
            await _logger.ErrorAsync(errorMessage, exception, customer);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Error(errorMessage);
        }
    }
}
