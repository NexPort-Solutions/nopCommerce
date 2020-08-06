using System.Linq;
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

namespace Nop.Plugin.Sale.PurchaseForCustomer.Controllers
{
    [ResponseCache(Duration = 0, NoStore = true)]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PurchaseForCustomerController : BasePluginController
    {
        private readonly IPurchaseForCustomerModelFactory _purchaseForCustomerModelFactory;
        private readonly IPurchaseForCustomerService _purchaseForCustomerService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public PurchaseForCustomerController(
            IPurchaseForCustomerModelFactory purchaseForCustomerModelFactory,
            IPurchaseForCustomerService purchaseForCustomerService,
            ICustomerService customerService,
            IProductService productService,
            IStoreService storeService,
            IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ILogger logger)
        {
            _purchaseForCustomerModelFactory = purchaseForCustomerModelFactory;
            _purchaseForCustomerService = purchaseForCustomerService;
            _customerService = customerService;
            _productService = productService;
            _storeService = storeService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _logger = logger;
        }

        public IActionResult PurchaseDetails(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = _purchaseForCustomerModelFactory.PreparePurchaseForCustomerOrderModel(productId);

            return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/PurchaseForCustomer/PurchaseDetails.cshtml", model);
        }

        [HttpPost]
        public IActionResult PurchaseForCustomer(PurchaseForCustomerOrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            PlaceOrderResult result = null;

            if (ModelState.IsValid)
            {
                var store = _storeService.GetStoreById(model.StoreId);
                if (store != null)
                {
                    var product = _productService.GetProductById(model.ProductId);
                    if (product != null)
                    {
                        foreach (var customerId in model.CustomerIds)
                        {
                            var customer = _customerService.GetCustomerById(customerId);
                            if (customer != null)
                            {
                                result = _purchaseForCustomerService.PurchaseProductForCustomer(product, customer, store, model.NotifyCustomer);
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
                        orderProcessingService.MarkOrderAsPaid(result.PlacedOrder);
                    }

                    ViewBag.OrderResultMessage = _localizationService.GetResource("Admin.Catalog.Products.PurchaseForCustomer.Success");
                }
                else
                {
                    ViewBag.OrderResultMessage = _localizationService.GetResource("Admin.Catalog.Products.PurchaseForCustomer.Error");

                    var logError = result.Errors.Aggregate("Error while placing order. ",
                        (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
                    foreach (var customerId in model.CustomerIds)
                    {
                        var customer = _customerService.GetCustomerById(customerId);
                        _logger.Error(logError, customer: customer);
                    }
                }
            }

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/PurchaseForCustomer/PurchaseDetails.cshtml", model);
        }
    }
}
