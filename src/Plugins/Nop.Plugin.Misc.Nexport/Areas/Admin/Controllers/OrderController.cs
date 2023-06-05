using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers
{
    public class OrderController : Web.Areas.Admin.Controllers.OrderController
    {
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IPermissionService _permissionService;

        public OrderController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper, IDownloadService downloadService,
            IEncryptionService encryptionService, IEventPublisher eventPublisher, IExportManager exportManager,
            IGiftCardService giftCardService, IImportManager importManager, ILocalizationService localizationService,
            INotificationService notificationService, IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService, IOrderService orderService, IPaymentService paymentService,
            IPdfService pdfService, IPermissionService permissionService,
            IPriceCalculationService priceCalculationService, IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService,
            IProductService productService, IShipmentService shipmentService, IShippingService shippingService,
            IShoppingCartService shoppingCartService, IStoreContext storeContext, IWorkContext workContext,
            IWorkflowMessageService workflowMessageService, OrderSettings orderSettings,
            INexportPluginModelFactory nexportPluginModelFactory) : base(addressAttributeParser, addressService,
            customerActivityService, customerService, dateTimeHelper, downloadService, encryptionService,
            eventPublisher, exportManager, giftCardService, importManager, localizationService, notificationService,
            orderModelFactory, orderProcessingService, orderService, paymentService, pdfService, permissionService,
            priceCalculationService, productAttributeFormatter, productAttributeParser, productAttributeService,
            productService, shipmentService, shippingService, shoppingCartService, storeContext, workContext,
            workflowMessageService, orderSettings)
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _orderModelFactory = orderModelFactory;
            _permissionService = permissionService;
        }

        [HttpGet]
        public override async Task<IActionResult> List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            {
                return AccessDeniedView();
            }
            var model = await _orderModelFactory.PrepareOrderSearchModelAsync(new OrderSearchModel
            {
                OrderStatusIds = orderStatuses,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses
            });
            return View(model);
        }

        [HttpPost]
        public override async Task<IActionResult> OrderList(OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _nexportPluginModelFactory.PrepareOrderListModelAsync(searchModel);

            return Json(model);
        }
    }
}
