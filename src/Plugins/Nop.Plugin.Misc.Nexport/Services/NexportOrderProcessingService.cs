using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportOrderProcessingService : OrderProcessingService
    {
        private readonly IOrderService _orderService;
        private readonly OrderSettings _orderSettings;
        private readonly NexportService _nexportService;

        #region Constructor

        public NexportOrderProcessingService(CurrencySettings currencySettings,
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
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
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
            NexportService nexportService) :
            base(currencySettings, addressService, affiliateService, checkoutAttributeFormatter, countryService, currencyService,
                customerActivityService, customerService, customNumberFormatter, discountService, encryptionService, eventPublisher,
                genericAttributeService, giftCardService, languageService, localizationService, logger, orderService, orderTotalCalculationService,
                paymentPluginManager, paymentService, pdfService, priceCalculationService, priceFormatter, productAttributeFormatter, productAttributeParser,
                productService, rewardPointService, shipmentService, shippingService, shoppingCartService, stateProvinceService, taxService, vendorService,
                webHelper, workContext, workflowMessageService, localizationSettings, orderSettings, paymentSettings,
                rewardPointsSettings, shippingSettings, taxSettings)
        {
            _orderService = orderService;
            _orderSettings = orderSettings;
            _nexportService = nexportService;
        }

        #endregion

        /// <summary>
        /// Check and set the order status.
        /// This will validate the order status based on the payment and shipping status of the order, then it will set the status according to the algorithm.
        /// If the order has any item that has Nexport mapping and is being processed by the scheduled task, then the status will not be set to complete.
        /// </summary>
        /// <param name="order">The order</param>
        public override void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatus == PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                // Ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            switch (order.OrderStatus)
            {
                case OrderStatus.Pending:
                    if (order.PaymentStatus == PaymentStatus.Authorized ||
                        order.PaymentStatus == PaymentStatus.Paid)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }

                    if (order.ShippingStatus == ShippingStatus.PartiallyShipped ||
                        order.ShippingStatus == ShippingStatus.Shipped ||
                        order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }

                    break;
                // Is order complete?
                case OrderStatus.Cancelled:
                case OrderStatus.Complete:
                    return;
            }

            if (order.PaymentStatus != PaymentStatus.Paid)
                return;

            bool completed;

            if (order.ShippingStatus == ShippingStatus.ShippingNotRequired)
            {
                // Shipping is not required
                completed = true;
            }
            else
            {
                // Shipping is required
                if (_orderSettings.CompleteOrderWhenDelivered)
                {
                    completed = order.ShippingStatus == ShippingStatus.Delivered;
                }
                else
                {
                    completed = order.ShippingStatus == ShippingStatus.Shipped ||
                                order.ShippingStatus == ShippingStatus.Delivered;
                }
            }

            var orderItems = _orderService.GetOrderItems(order.Id);

            // Check if the order has any item that has Nexport mapping
            var hasAnyNexportProduct = orderItems
                .Select(item => _nexportService.GetProductMappingByNopProductId(item.ProductId))
                .Any(productMapping => productMapping != null);

            // If the order contains Nexport product and is being processed, then do not set the status to complete
            if (hasAnyNexportProduct)
            {
                if (_nexportService.HasNexportOrderProcessingQueueItem(order.Id))
                {
                    completed = false;
                }
            }

            if (completed)
            {
                SetOrderStatus(order, OrderStatus.Complete, true);
            }
        }
    }
}
