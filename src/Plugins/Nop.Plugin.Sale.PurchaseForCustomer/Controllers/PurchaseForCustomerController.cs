using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Plugin.Sale.PurchaseForCustomer.Factories;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Services.Configuration;
using Nop.Services.Helpers;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Controllers;

[ResponseCache(Duration = 0, NoStore = true)]
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
public class PurchaseForCustomerController : BasePluginController
{
    private readonly PurchaseForCustomerPluginService _purchaseForCustomerPluginService;
    private readonly IPurchaseForCustomerModelFactory _purchaseForCustomerModelFactory;
    private readonly IPurchaseForCustomerService _purchaseForCustomerService;
    private readonly ICustomerService _customerService;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ICustomerActivityService _customerActivityService;

    public PurchaseForCustomerController(
        PurchaseForCustomerPluginService purchaseForCustomerPluginService,
        IPurchaseForCustomerModelFactory purchaseForCustomerModelFactory,
        IPurchaseForCustomerService purchaseForCustomerService,
        ICustomerService customerService,
        ISettingService settingService,
        IProductService productService,
        IStoreService storeService,
        IPermissionService permissionService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        ILogger logger,
        IDateTimeHelper dateTimeHelper,
        ICustomerActivityService customerActivityService)
    {
        _purchaseForCustomerPluginService = purchaseForCustomerPluginService;
        _purchaseForCustomerModelFactory = purchaseForCustomerModelFactory;
        _purchaseForCustomerService = purchaseForCustomerService;
        _customerService = customerService;
        _settingService = settingService;
        _productService = productService;
        _storeService = storeService;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _logger = logger;
        _dateTimeHelper = dateTimeHelper;
        _customerActivityService = customerActivityService;
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetModifiedLocaleResources(PurchaseForCustomerPluginResourceListSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return await AccessDeniedJsonAsync();

        var model = await _purchaseForCustomerModelFactory.PreparePurchaseForCustomerPluginResourceListModelAsync(searchModel);

        return Json(model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> OverrideResources(ICollection<int> selectedIds, bool allChecked)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
            return AccessDeniedView();

        if (selectedIds != null && selectedIds.Count != 0)
        {
            foreach (var id in selectedIds)
            {
                var localeStringResourceById = await _localizationService.GetLocaleStringResourceByIdAsync(id);

                if (localeStringResourceById != null)
                {
                    var purchaseForCustomerLocaleResources = _purchaseForCustomerPluginService.GetLocaleResources();
                    var overridingResourceValue = purchaseForCustomerLocaleResources
                        .Where(l => string.Equals(l.Key, localeStringResourceById.ResourceName, StringComparison.CurrentCultureIgnoreCase))
                        .Select(l => l.Value)
                        .First();

                    if (overridingResourceValue != null)
                    {
                        localeStringResourceById.ResourceValue = overridingResourceValue;

                        await _localizationService.UpdateLocaleStringResourceAsync(localeStringResourceById);
                    }
                }
            }

            if (allChecked)
            {
                var purchaseForCustomerSetting = await _settingService.GetSettingAsync("Plugin.Sale.PurchaseForCustomer.HasModifiedLocaleResources");

                if (purchaseForCustomerSetting != null)
                {
                    await _settingService.DeleteSettingAsync(purchaseForCustomerSetting);
                }
            }
        }
        else
        {
            return NoContent();
        }

        return Json(new { success = true });
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public async Task<IActionResult> PurchaseDetails(int productId)
    {
        var model = await _purchaseForCustomerModelFactory.PreparePurchaseForCustomerOrderModel(productId);

        return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/PurchaseForCustomer/PurchaseDetails.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Orders.ORDERS_VIEW)]
    public async Task<IActionResult> PurchaseForCustomer(PurchaseForCustomerOrderModel model)
    {
        PlaceOrderResult result = null;

        if (ModelState.IsValid)
        {
            var store = await _storeService.GetStoreByIdAsync(model.StoreId);
            if (store != null)
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product != null)
                {
                    foreach (var customerId in model.CustomerIds)
                    {
                        var customer = await _customerService.GetCustomerByIdAsync(customerId);
                        if (customer != null)
                        {
                            var utcStartDate = model.StartDate.HasValue ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value) : null;
                            result = await _purchaseForCustomerService
                                .PurchaseProductForCustomerAsync(product, customer, store, utcStartDate, model.NotifyCustomer);

                            if (result is { Success: true })
                            {
                                await _customerActivityService.InsertActivityAsync(PluginDefaults.NEXPORT_PURCHASE_PRODUCT_FOR_CUSTOMER,
                                    $"Purchased product [{product.Name} ({product.Id})] for customer #{customer.Id}", product);
                            }
                        }
                    }
                }
            }
        }

        if (result != null)
        {
            if (result.Success)
            {
                if (model.MarkOrderAsPaid)
                {
                    var orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();
                    await orderProcessingService.MarkOrderAsPaidAsync(result.PlacedOrder);
                }

                ViewBag.OrderResultMessage = await _localizationService.GetResourceAsync("Admin.Catalog.Products.PurchaseForCustomer.Success");
            }
            else
            {
                ViewBag.OrderResultMessage = await _localizationService.GetResourceAsync("Admin.Catalog.Products.PurchaseForCustomer.Error");

                var logError = result.Errors.Aggregate("Error while placing order. ",
                    (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
                foreach (var customerId in model.CustomerIds)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(customerId);
                    await _logger.ErrorAsync(logError, customer: customer);
                }
            }
        }

        ViewBag.RefreshPage = true;

        return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/PurchaseForCustomer/PurchaseDetails.cshtml", model);
    }

    public virtual async Task<IActionResult> SearchCustomers(string term)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Customers.CUSTOMERS_VIEW))
            return Content(string.Empty);

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        var customers = await _purchaseForCustomerService.SearchCustomersAsync(term);

        var result = customers.Select(c=> new{
            label=$"{c.FirstName} {c.LastName} ({c.Email})",
            customer=new CustomerModel()
            {
                FullName=$"{c.FirstName} {c.LastName}",
                Email = c.Email,
                Id = c.Id
            }
        }).ToList();

        return Json(result);
    }
}