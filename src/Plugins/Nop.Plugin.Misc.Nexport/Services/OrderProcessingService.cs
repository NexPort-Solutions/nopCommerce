using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.Nexport.Services;

public class OrderProcessingService : Nop.Services.Orders.OrderProcessingService
{
    private readonly IOrderService _order;
    private readonly OrderSettings _orderSettings;
    private readonly IInvoiceService _invoiceService;
    private readonly IOrderProcessingQueueItemService _orderProcessingQueueItemService;
    private readonly IProductMappingService _productMappingService;

    #region Constructor
#pragma warning disable CA1506
    public OrderProcessingService(
        CurrencySettings currencySettings,
        IAddressService addressService,
        IAffiliateService affiliateService,
        ICheckoutAttributeFormatter checkoutAttributeFormatter,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IDiscountService discountService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILogger logger,
        IOrderService orderService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        IReturnRequestService returnRequestService,
        IRewardPointService rewardPointService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreService storeService,
        ITaxService taxService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IInvoiceService invoiceService,
        IOrderProcessingQueueItemService orderProcessingQueueItemService,
        IProductMappingService productMappingService)
#pragma warning restore CA1506
        : base(
            currencySettings,
            addressService,
            affiliateService,
            checkoutAttributeFormatter,
            countryService,
            currencyService,
            customerActivityService,
            customerService,
            customNumberFormatter,
            discountService,
            encryptionService,
            eventPublisher,
            genericAttributeService,
            giftCardService,
            languageService,
            localizationService,
            logger,
            orderService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pdfService,
            priceCalculationService,
            priceFormatter,
            productAttributeFormatter,
            productAttributeParser,
            productService,
            returnRequestService,
            rewardPointService,
            shipmentService,
            shippingService,
            shoppingCartService,
            stateProvinceService,
            storeService,
            taxService,
            vendorService,
            webHelper,
            workContext,
            workflowMessageService,
            localizationSettings,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings)
    {
        _order = orderService;
        _orderSettings = orderSettings;
        _invoiceService = invoiceService;
        _orderProcessingQueueItemService = orderProcessingQueueItemService;
        _productMappingService = productMappingService;
    }

    #endregion Constructor

    /// <summary>
    /// Check and set the order status.
    /// This will validate the order status based on the payment and shipping status of the order, then it will set the status according to the algorithm.
    /// If the order has any item that has NexPort mapping and is being processed by the scheduled task, then the status will not be set to complete.
    /// </summary>
    /// <param name="order"></param>
    public override async Task CheckOrderStatusAsync(Order order)
    {
        if (order.PaymentStatus is PaymentStatus.Paid && order.PaidDateUtc is null)
        {
            order.PaidDateUtc = DateTime.UtcNow;
            await _order.UpdateOrderAsync(order);
        }
        // TODO this might set the order status twice - probably unnecessarily.
        switch (order.OrderStatus)
        {
            case OrderStatus.Pending
                when order.PaymentStatus is PaymentStatus.Authorized or PaymentStatus.Paid
                    || order.ShippingStatus is ShippingStatus.PartiallyShipped or ShippingStatus.Shipped or ShippingStatus.Delivered:
            {
                await SetOrderStatusAsync(order, OrderStatus.Processing, false);
                break;
            }
            case OrderStatus.Cancelled or OrderStatus.Complete:
                return;
            default: // unreachable
                throw new NotImplementedException();
        }
        if (order.PaymentStatus is not PaymentStatus.Paid)
        {
            return;
        }
        if (!await OrderContainsProcessingNexPortProduct(order) && ShippingIsCompleted(order))
        {
            await SetOrderStatusAsync(order, OrderStatus.Complete, true);
        }
    }

    private bool ShippingIsCompleted(Order order)
        => order.ShippingStatus is ShippingStatus.ShippingNotRequired || RequiredShippingIsCompleted(order);

    private async Task<bool> OrderContainsProcessingNexPortProduct(Order order)
        => await HasAnyNexportProduct(await _order.GetOrderItemsAsync(order.Id))
            && (await _orderProcessingQueueItemService.HasOrderProcessingQueueItem(order.Id)
                || (await _invoiceService.GetOrderInvoiceItems(order.Id, true)).Count > 0);

    private async Task<bool> HasAnyNexportProduct(IList<OrderItem> orderItems)
        => await orderItems
            .SelectAwait(async item => await _productMappingService.GetByNopProductId(item.ProductId))
            .AnyAsync(productMapping => productMapping is not null);

    private bool RequiredShippingIsCompleted(Order order)
        => _orderSettings.CompleteOrderWhenDelivered
            ? order.ShippingStatus is ShippingStatus.Delivered
            : order.ShippingStatus is ShippingStatus.Shipped or ShippingStatus.Delivered;
}
