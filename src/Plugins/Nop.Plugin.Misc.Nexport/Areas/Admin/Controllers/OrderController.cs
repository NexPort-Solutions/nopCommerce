using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Catalog;
using Nop.Services.Common;
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
using Nop.Web.Areas.Admin.Models.Orders;
using Newtonsoft.Json;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Events;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
[Route(template: "[area]/[controller]/[action]", Order = int.MinValue)]
public class OrderController : Web.Areas.Admin.Controllers.OrderController, IConsumer<OrderPlacedEvent>
{
    private readonly IProductGroupMembershipService _productGroupMembership;
    private readonly IProductMappingService _productMapping;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _store;
    private readonly IOrderService _order;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IPluginModelFactory _model;
    private readonly IPermissionService _permission;
    private readonly IOrderProcessingQueueItemService _orderProcessingQueueItem;

    public OrderController(
        IAddressAttributeParser addressAttributeParser,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IDownloadService downloadService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IGiftCardService giftCardService,
        IImportManager importManager,
        ILocalizationService localizationService,
        INotificationService notificationService,
        Web.Areas.Admin.Factories.IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPermissionService permission,
        IPriceCalculationService priceCalculationService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        OrderSettings orderSettings,
        IPluginModelFactory model,
        IOrderProcessingQueueItemService orderProcessingQueueItem,
        IProductGroupMembershipService productGroupMembership,
        IProductMappingService productMapping,
        IStoreService store,
        IOrderService order,
        IGenericAttributeService genericAttribute)
        : base(
            addressAttributeParser,
            addressService,
            customerActivityService,
            customerService,
            dateTimeHelper,
            downloadService,
            encryptionService,
            eventPublisher,
            exportManager,
            giftCardService,
            importManager,
            localizationService,
            notificationService,
            orderModelFactory,
            orderProcessingService,
            orderService,
            paymentService,
            pdfService,
            permission,
            priceCalculationService,
            productAttributeFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            shipmentService,
            shippingService,
            shoppingCartService,
            storeContext,
            workContext,
            workflowMessageService,
            orderSettings)
    {
        _productGroupMembership = productGroupMembership;
        _productMapping = productMapping;
        _workContext = workContext;
        _storeContext = storeContext;
        _store = store;
        _order = order;
        _genericAttribute = genericAttribute;
        _model = model;
        _permission = permission;
        _orderProcessingQueueItem = orderProcessingQueueItem;
    }

    [HttpPost]
    public override async Task<IActionResult> OrderList(OrderSearchModel searchModel)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.OrderListModel(searchModel));
    }

    [HttpPost]
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage.Order;
        var orderItems = await _order.GetOrderItemsAsync(order.Id);
        foreach (var item in orderItems)
        {
            var mapping = await _productMapping.GetByNopProductId(item.ProductId, order.StoreId)
                ?? await _productMapping.GetByNopProductId(item.ProductId);
            if (mapping is null)
            {
                continue;
            }
            var store = await _store.GetStoreByIdAsync(order.StoreId);
            var storeModel = await _genericAttribute.GetAttributeAsync<StoreSaleModel>(store, STORE_SALE_MODEL_SETTING_KEY, store.Id);
            await _genericAttribute.SaveAttributeAsync(item, $"StoreModel-{order.Id}-{item.Id}", JsonConvert.SerializeObject(storeModel), store.Id);
            await _genericAttribute.SaveAttributeAsync(item, $"ProductMapping-{order.Id}-{item.Id}", JsonConvert.SerializeObject(mapping), order.StoreId);
            var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var membershipMapping in groupMembershipMappings)
            {
                await _genericAttribute.SaveAttributeAsync(item, $"ProductGroupMembershipMapping-{order.Id}-{item.Id}-{mapping.Id}", JsonConvert.SerializeObject(membershipMapping), order.StoreId);
            }
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        var orderStore = await _storeContext.GetCurrentStoreAsync();
        var groupForCustomer = await _genericAttribute.GetAttributeAsync<string>(customer, GROUP_FOR_CUSTOMER, orderStore.Id);
        // reset generic attribute group for customer for future purchases
        await _genericAttribute.SaveAttributeAsync<string>(customer, GROUP_FOR_CUSTOMER, null!, orderStore.Id);
        // set attribute group for order
        await _genericAttribute.SaveAttributeAsync(order, GROUP_FOR_ORDER, groupForCustomer, orderStore.Id);
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
    {
        if (eventMessage.Entity is not { OrderStatus: OrderStatus.Processing, PaymentStatus: PaymentStatus.Paid })
        {
            return;
        }
        await ProcessNewRedemption(eventMessage.Entity);
    }

    [HttpPost]
    public async Task ProcessNewRedemption(Order order)
    {
        await _orderProcessingQueueItem.Insert(new OrderProcessingQueueItem
            {
                OrderId = order.Id,
                UtcDateCreated = DateTime.UtcNow,
            });
    }
}
