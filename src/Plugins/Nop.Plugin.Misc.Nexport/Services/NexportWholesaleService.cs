using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using System.Globalization;

#nullable enable

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportWholesaleService
{
    Task<PlaceOrderResult> PlaceWholesaleOrderAsync(ProcessPaymentRequest processPaymentRequest, List<ShoppingCartItem> shoppingCartItems);
}

public class NexportNexportWholesaleService : INexportWholesaleService
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly IAddressService _addressService;
    private readonly IAffiliateService _affiliateService;
    private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
    private readonly ICountryService _countryService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly ICustomNumberFormatter _customNumberFormatter;
    private readonly IDiscountService _discountService;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IGiftCardService _giftCardService;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly IOrderService _orderService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPaymentService _paymentService;
    private readonly IPdfService _pdfService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductService _productService;
    private readonly IRewardPointService _rewardPointService;
    private readonly IShippingService _shippingService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IStoreService _storeService;
    private readonly ITaxService _taxService;
    private readonly IVendorService _vendorService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly LocalizationSettings _localizationSettings;
    private readonly OrderSettings _orderSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly TaxSettings _taxSettings;
    #endregion

    #region Constructors

    public NexportNexportWholesaleService(
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
        TaxSettings taxSettings)
    {
        _currencySettings = currencySettings;
        _addressService = addressService;
        _affiliateService = affiliateService;
        _checkoutAttributeFormatter = checkoutAttributeFormatter;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _customNumberFormatter = customNumberFormatter;
        _discountService = discountService;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _giftCardService = giftCardService;
        _languageService = languageService;
        _localizationService = localizationService;
        _logger = logger;
        _orderService = orderService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _pdfService = pdfService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeParser = productAttributeParser;
        _productService = productService;
        _rewardPointService = rewardPointService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _stateProvinceService = stateProvinceService;
        _storeService = storeService;
        _taxService = taxService;
        _vendorService = vendorService;
        _webHelper = webHelper;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _localizationSettings = localizationSettings;
        _orderSettings = orderSettings;
        _shippingSettings = shippingSettings;
        _taxSettings = taxSettings;
    }

    #endregion

    public async Task<PlaceOrderResult> PlaceWholesaleOrderAsync(ProcessPaymentRequest processPaymentRequest, List<ShoppingCartItem> shoppingCartItems)
    {
        var result = new PlaceOrderResult();
        if (processPaymentRequest.OrderGuid == Guid.Empty)
        {
            result.AddError("Order GUID is not generated");
        }
        else
        {
            var details = await PreparePlaceOrderForCustomerDetailsAsync(processPaymentRequest, shoppingCartItems);
            var processPaymentResult = await GetProcessPaymentResultAsync(processPaymentRequest, details);
            if (processPaymentResult is null or { Success: false })
            {
                await handleUnsuccessfulPayment(processPaymentResult);
            }
            else
            {
                await handleSuccessfulPayment(processPaymentResult, details);
            }
        }
        if (!result.Success)
            await logOrderError();
        return result;

        async Task handleUnsuccessfulPayment(ProcessPaymentResult? processPaymentResult)
        {
            result.AddError($"{nameof(processPaymentResult)} is not available");
            if (processPaymentResult?.Errors is { } errors)
            {
                foreach (var paymentError in errors)
                {
                    var resource = await _localizationService.GetResourceAsync("Checkout.PaymentError");
                    result.AddError(string.Format(resource, paymentError));
                }
            }
        }

        async Task logOrderError()
        {
            var logError = result.Errors.Aggregate("Error while placing order. ",
                (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            await _logger.ErrorAsync(logError, customer: customer);
        }

        async Task handleSuccessfulPayment(ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            try
            {
                var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                result.PlacedOrder = order;
                await MoveTempShoppingCartItemToOrderItemsAsync(details, order);
                var currentOrderProcessor = EngineContext.Current.Resolve<IOrderProcessingService>();
                await currentOrderProcessor.CheckOrderStatusAsync(order);
                await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));
                if (order.PaymentStatus is PaymentStatus.Paid)
                {
                    await ProcessOrderPaidAsync(order);
                }
            }
            catch (NopException e)
            {
                result.AddError(e.Message);
            }
        }
    }

    private async Task<ProcessPaymentResult> GetProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
    {
        var processPaymentResult = await IsPaymentWorkflowRequiredAsync(details.Cart)
            ? await ProcessPaymentResultAsync(processPaymentRequest, details)
            : new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
        return processPaymentResult;
    }

    private async Task<ProcessPaymentResult> ProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
    {
        var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(processPaymentRequest.PaymentMethodSystemName, customer, processPaymentRequest.StoreId);
        if (paymentMethod is null)
            throw new NopException("Payment method couldn't be loaded");
        if (!_paymentPluginManager.IsPluginActive(paymentMethod))
            throw new NopException("Payment method is not active");

        var result = details.IsRecurringShoppingCart ?
            await ProcessRecurringPaymentAsync(processPaymentRequest)
            : await _paymentService.ProcessPaymentAsync(processPaymentRequest);
        return result;
    }

    private async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var recurringPaymentType = await _paymentService.GetRecurringPaymentTypeAsync(processPaymentRequest.PaymentMethodSystemName);
        return recurringPaymentType switch
        {
            RecurringPaymentType.NotSupported => throw new NopException("Recurring payments are not supported by selected payment method"),
            RecurringPaymentType.Manual or RecurringPaymentType.Automatic => await _paymentService.ProcessRecurringPaymentAsync(processPaymentRequest),
            _ => throw new NopException("Not supported recurring payment type"),
        };
    }

    private async Task<bool> IsPaymentWorkflowRequiredAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
    {
        var shoppingCartTotalDetails = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, useRewardPoints: useRewardPoints);
        return shoppingCartTotalDetails.shoppingCartTotal is not decimal.Zero;
    }

    private async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
    {
        if (details.BillingAddress is null)
            throw new NopException("Billing address is not provided");

        var order = new Order
        {
            StoreId = processPaymentRequest.StoreId,
            OrderGuid = processPaymentRequest.OrderGuid,
            CustomerId = details.Customer.Id,
            CustomerLanguageId = details.CustomerLanguage.Id,
            CustomerTaxDisplayType = details.CustomerTaxDisplayType,
            CustomerIp = _webHelper.GetCurrentIpAddress(),
            OrderSubtotalInclTax = details.OrderSubTotalInclTax,
            OrderSubtotalExclTax = details.OrderSubTotalExclTax,
            OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
            OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
            OrderShippingInclTax = details.OrderShippingTotalInclTax,
            OrderShippingExclTax = details.OrderShippingTotalExclTax,
            PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
            PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
            TaxRates = details.TaxRates,
            OrderTax = details.OrderTaxTotal,
            OrderTotal = details.OrderTotal,
            RefundedAmount = decimal.Zero,
            OrderDiscount = details.OrderDiscountAmount,
            CheckoutAttributeDescription = details.CheckoutAttributeDescription,
            CheckoutAttributesXml = details.CheckoutAttributesXml,
            CustomerCurrencyCode = details.CustomerCurrencyCode,
            CurrencyRate = details.CustomerCurrencyRate,
            AffiliateId = details.AffiliateId,
            OrderStatus = OrderStatus.Pending,
            AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
            CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
            CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
            CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
            MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
            CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
            CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
            CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
            PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
            AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
            AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
            AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
            CaptureTransactionId = processPaymentResult.CaptureTransactionId,
            CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
            SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
            PaymentStatus = processPaymentResult.NewPaymentStatus,
            PaidDateUtc = null,
            PickupInStore = details.PickupInStore,
            ShippingStatus = details.ShippingStatus,
            ShippingMethod = details.ShippingMethodName,
            ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
            CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
            VatNumber = details.VatNumber,
            CreatedOnUtc = DateTime.UtcNow,
            CustomOrderNumber = string.Empty
        };
        await _addressService.InsertAddressAsync(details.BillingAddress);
        order.BillingAddressId = details.BillingAddress.Id;
        if (details.PickupAddress != null)
        {
            await _addressService.InsertAddressAsync(details.PickupAddress);
            order.PickupAddressId = details.PickupAddress.Id;
        }
        if (details.ShippingAddress != null)
        {
            await _addressService.InsertAddressAsync(details.ShippingAddress);
            order.ShippingAddressId = details.ShippingAddress.Id;
        }
        await _orderService.InsertOrderAsync(order);
        //generate and set custom order number
        order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
        await _orderService.UpdateOrderAsync(order);
        //reward points history
        if (details.RedeemedRewardPointsAmount <= decimal.Zero)
            return order;
        var message = string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber);
        order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId, message, order, details.RedeemedRewardPointsAmount);
        await _customerService.UpdateCustomerAsync(details.Customer);
        await _orderService.UpdateOrderAsync(order);
        return order;
    }

    private async Task ProcessOrderPaidAsync(Order order)
    {
        await _eventPublisher.PublishAsync(new OrderPaidEvent(order));
        //order paid email notification
        if (order.OrderTotal != decimal.Zero)
        {
            //we should not send it for free ($0 total) orders?
            //remove this "if" statement if you want to send it in this case
            var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ? await _pdfService.SaveOrderPdfToDiskAsync(order) : null;
            var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ? string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf" : null;
            var orderPaidCustomerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidCustomerNotificationAsync(order, order.CustomerLanguageId, orderPaidAttachmentFilePath, orderPaidAttachmentFileName);
            if (orderPaidCustomerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, buildNote("customer", orderPaidCustomerNotificationQueuedEmailIds));
            var orderPaidStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPaidStoreOwnerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, buildNote("store owner", orderPaidStoreOwnerNotificationQueuedEmailIds));
            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPaidVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidVendorNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, buildNote("vendor", orderPaidVendorNotificationQueuedEmailIds));
            }
            if (order.AffiliateId != 0)
            {
                var orderPaidAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidAffiliateNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, buildNote("affiliate", orderPaidAffiliateNotificationQueuedEmailIds));
            }
        }

        //customer roles with "purchased with product" specified
        await ProcessCustomerRolesWithPurchasedProductSpecifiedAsync(order, true);

        string buildNote(string emailToEntityName, IEnumerable<int> emailIds)
        {
            var emailIdsFormatted = string.Join(", ", emailIds);
            return $"\"Order paid\" email (to {emailToEntityName}) has been queued. Queued email identifiers: {emailIdsFormatted}.";
        }
    }

    private async Task AddOrderNoteAsync(Order order, string note)
    {
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });
    }


    private async Task ProcessCustomerRolesWithPurchasedProductSpecifiedAsync(Order order, bool add)
    {
        //purchased product identifiers
        var purchasedProductIds = new List<int>();
        foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
        {
            //standard items
            purchasedProductIds.Add(orderItem.ProductId);
            //bundled (associated) products
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);
            purchasedProductIds.AddRange(attributeValues
                .Where(attributeValue => attributeValue.AttributeValueType is AttributeValueType.AssociatedToProduct)
                .Select(attributeValue => attributeValue.AssociatedProductId));
        }

        var customerRoles = (await _customerService
                .GetAllCustomerRolesAsync(true))
            .Where(customerRole => purchasedProductIds.Contains(customerRole.PurchasedWithProductId))
            .ToList();
        if (!customerRoles.Any())
            return;

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        foreach (var customerRole in customerRoles)
            if (!await _customerService.IsInCustomerRoleAsync(customer, customerRole.SystemName))
                if (add)
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                else if (!add)
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
        await _customerService.UpdateCustomerAsync(customer);
    }

    private async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
    {
        var pIds = (await _orderService.GetOrderItemsAsync(order.Id)).Select(x => x.ProductId).ToArray();
        return await _vendorService.GetVendorsByProductIdsAsync(pIds);
    }

    private async Task SendNotificationsToCustomerAndSaveNotesAsync(Order order)
    {
        await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
            ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
            : "Order placed");
        var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

        var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ? await _pdfService.SaveOrderPdfToDiskAsync(order) : null;
        var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ? string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf" : null;
        var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
        if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

        var vendors = await GetVendorsInOrderAsync(order);
        foreach (var vendor in vendors)
        {
            var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
        }

        if (order.AffiliateId is 0)
            return;

        var orderPlacedAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");
    }

    private async Task MoveTempShoppingCartItemToOrderItemsAsync(PlaceOrderContainer details, Order order)
    {
        foreach (var sc in details.Cart)
        {
            var product = await _productService.GetProductByIdAsync(sc.ProductId);
            var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
            var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
            var scUnitPriceInclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, true, details.Customer);
            var scUnitPriceExclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, false, details.Customer);
            var scSubTotalInclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, true, details.Customer);
            var scSubTotalExclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, false, details.Customer);
            var discountAmountInclTax = await _taxService.GetProductPriceAsync(product, discountAmount, true, details.Customer);
            var discountAmountExclTax = await _taxService.GetProductPriceAsync(product, discountAmount, false, details.Customer);
            details.AppliedDiscounts.AddRange(scDiscounts.Where(disc => !_discountService.ContainsDiscount(details.AppliedDiscounts, disc)));
            var store = await _storeService.GetStoreByIdAsync(sc.StoreId);
            var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, details.Customer, store);
            var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);
            var orderItem = new OrderItem
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPriceInclTax = scUnitPriceInclTax.price,
                UnitPriceExclTax = scUnitPriceExclTax.price,
                PriceInclTax = scSubTotalInclTax.price,
                PriceExclTax = scSubTotalExclTax.price,
                OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, sc.AttributesXml),
                AttributeDescription = attributeDescription,
                AttributesXml = sc.AttributesXml,
                Quantity = sc.Quantity,
                DiscountAmountInclTax = discountAmountInclTax.price,
                DiscountAmountExclTax = discountAmountExclTax.price,
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = 0,
                ItemWeight = itemWeight,
                RentalStartDateUtc = sc.RentalStartDateUtc,
                RentalEndDateUtc = sc.RentalEndDateUtc
            };
            await _orderService.InsertOrderItemAsync(orderItem);
            await AddGiftCardsAsync(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax.price);
            await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml, string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
        }

        //clear shopping cart
        await Task.WhenAll(details.Cart.Select(sci => _shoppingCartService.DeleteShoppingCartItemAsync(sci, false)));
    }

    private async Task AddGiftCardsAsync(Product product, string attributesXml, int quantity, OrderItem orderItem, decimal? unitPriceExclTax = null, decimal? amount = null)
    {
        if (!product.IsGiftCard)
            return;

        _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName, out var giftCardRecipientEmail, out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);
        for (var i = 0; i < quantity; i++)
            await _giftCardService.InsertGiftCardAsync(new GiftCard
            {
                GiftCardType = product.GiftCardType,
                PurchasedWithOrderItemId = orderItem.Id,
                Amount = amount ?? product.OverriddenGiftCardAmount ?? unitPriceExclTax ?? 0,
                IsGiftCardActivated = false,
                GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                RecipientName = giftCardRecipientName,
                RecipientEmail = giftCardRecipientEmail,
                SenderName = giftCardSenderName,
                SenderEmail = giftCardSenderEmail,
                Message = giftCardMessage,
                IsRecipientNotified = false,
                CreatedOnUtc = DateTime.UtcNow
            });
    }

    private class PlaceOrderContainer
    {
        public Customer? Customer { get; set; }
        public Language? CustomerLanguage { get; set; }
        public int AffiliateId { get; set; }
        public TaxDisplayType CustomerTaxDisplayType { get; set; }
        public string? CustomerCurrencyCode { get; set; }
        public decimal CustomerCurrencyRate { get; set; }
        public Address? BillingAddress { get; set; }
        public Address? ShippingAddress { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public string? ShippingMethodName { get; set; }
        public string? ShippingRateComputationMethodSystemName { get; set; }
        public bool PickupInStore { get; set; }
        public Address? PickupAddress { get; set; }
        public bool IsRecurringShoppingCart { get; set; }
        public Order? InitialOrder { get; set; }
        public string? CheckoutAttributeDescription { get; set; }
        public string? CheckoutAttributesXml { get; set; }
        public List<ShoppingCartItem> Cart { get; set; } = new List<ShoppingCartItem>();
        public List<Discount> AppliedDiscounts { get; set; } = new List<Discount>();
        public List<AppliedGiftCard> AppliedGiftCards { get; set; } = new List<AppliedGiftCard>();
        public decimal OrderSubTotalInclTax { get; set; }
        public decimal OrderSubTotalExclTax { get; set; }
        public decimal OrderSubTotalDiscountInclTax { get; set; }
        public decimal OrderSubTotalDiscountExclTax { get; set; }
        public decimal OrderShippingTotalInclTax { get; set; }
        public decimal OrderShippingTotalExclTax { get; set; }
        public decimal PaymentAdditionalFeeInclTax { get; set; }
        public decimal PaymentAdditionalFeeExclTax { get; set; }
        public decimal OrderTaxTotal { get; set; }
        public string? VatNumber { get; set; }
        public string? TaxRates { get; set; }
        public decimal OrderDiscountAmount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public decimal RedeemedRewardPointsAmount { get; set; }
        public decimal OrderTotal { get; set; }
    }

    private async Task PrepareAndValidateBillingAddressAsync(PlaceOrderContainer details)
    {
        if (details.Customer.BillingAddressId is null)
            throw new NopException("Billing address is not provided");

        var billingAddress = await _customerService.GetCustomerBillingAddressAsync(details.Customer);
        if (!CommonHelper.IsValidEmail(billingAddress?.Email))
            throw new NopException("Email is not valid");

        details.BillingAddress = _addressService.CloneAddress(billingAddress);
        if (await _countryService.GetCountryByAddressAsync(details.BillingAddress) is { AllowsBilling: false } billingCountry)
            throw new NopException($"Country '{billingCountry.Name}' is not allowed for billing");
    }

    private async Task PrepareAndValidateShippingInfoAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest)
    {
        //shipping info
        if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(details.Cart))
        {
            var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(details.Customer, NopCustomerDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
            if (_shippingSettings.AllowPickupInStore && pickupPoint != null)
            {
                var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);
                details.PickupInStore = true;
                details.PickupAddress = new Address
                {
                    Address1 = pickupPoint.Address,
                    City = pickupPoint.City,
                    County = pickupPoint.County,
                    CountryId = country?.Id,
                    StateProvinceId = state?.Id,
                    ZipPostalCode = pickupPoint.ZipPostalCode,
                    CreatedOnUtc = DateTime.UtcNow
                };
            }
            else
            {
                if (details.Customer.ShippingAddressId is null)
                    throw new NopException("Shipping address is not provided");

                var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(details.Customer);
                if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                    throw new NopException("Email is not valid");

                //clone shipping address
                details.ShippingAddress = _addressService.CloneAddress(shippingAddress);
                if (await _countryService.GetCountryByAddressAsync(details.ShippingAddress) is Country
                    {
                        AllowsShipping: false
                    } shippingCountry)
                    throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
            }

            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(details.Customer, NopCustomerDefaults.SelectedShippingOptionAttribute, processPaymentRequest.StoreId);
            if (shippingOption != null)
            {
                details.ShippingMethodName = shippingOption.Name;
                details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
            }

            details.ShippingStatus = ShippingStatus.NotYetShipped;
        }
        else
            details.ShippingStatus = ShippingStatus.ShippingNotRequired;
    }

    private async Task PrepareAndValidateTotalsAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest)
    {
        var (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax, subTotalWithoutDiscountExclTax, _, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalsAsync(details.Cart);
        //sub total (incl tax)
        details.OrderSubTotalInclTax = subTotalWithoutDiscountInclTax;
        details.OrderSubTotalDiscountInclTax = discountAmountInclTax;
        //discount history
        details.AppliedDiscounts.AddRange(appliedDiscounts.Where(disc => !_discountService.ContainsDiscount(details.AppliedDiscounts, disc)));

        //sub total (excl tax)
        details.OrderSubTotalExclTax = subTotalWithoutDiscountExclTax;
        details.OrderSubTotalDiscountExclTax = discountAmountExclTax;
        //shipping total
        var (orderShippingTotalInclTax, orderShippingTotalExclTax, _, shippingTotalDiscounts) = await _orderTotalCalculationService.GetShoppingCartShippingTotalsAsync(details.Cart);
        if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
            throw new NopException("Shipping total couldn't be calculated");

        details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
        details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;
        foreach (var disc in shippingTotalDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);

        //payment total
        var paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(details.Cart, processPaymentRequest.PaymentMethodSystemName);
        details.PaymentAdditionalFeeInclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, true, details.Customer)).price;
        details.PaymentAdditionalFeeExclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, false, details.Customer)).price;
        //tax amount
        SortedDictionary<decimal, decimal> taxRatesDictionary;
        (details.OrderTaxTotal, taxRatesDictionary) = await _orderTotalCalculationService.GetTaxTotalAsync(details.Cart);
        //VAT number
        if (_taxSettings.EuVatEnabled && details.Customer.VatNumberStatus is VatNumberStatus.Valid)
            details.VatNumber = details.Customer.VatNumber;

        //tax rates
        details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
            $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");
        //order total (and applied discounts, gift cards, reward points)
        var (orderTotal, orderDiscountAmount, orderAppliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(details.Cart);
        if (!orderTotal.HasValue)
            throw new NopException("Order total couldn't be calculated");

        details.OrderDiscountAmount = orderDiscountAmount;
        details.RedeemedRewardPoints = redeemedRewardPoints;
        details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
        details.AppliedGiftCards = appliedGiftCards;
        details.OrderTotal = orderTotal.Value;
        //discount history
        foreach (var disc in orderAppliedDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);
        processPaymentRequest.OrderTotal = details.OrderTotal;
    }

    private async Task<PlaceOrderContainer> PreparePlaceOrderForCustomerDetailsAsync(ProcessPaymentRequest processPaymentRequest, List<ShoppingCartItem> shoppingCartItems)
    {
        var details = new PlaceOrderContainer { Cart = shoppingCartItems };
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        await PrepareAndValidateCustomerAsync(details, processPaymentRequest, currentCurrency);
        await PrepareAndValidateCheckoutAttributesAsync(details, processPaymentRequest, currentCurrency);
        await PrepareAndValidateBillingAddressAsync(details);
        await PrepareAndValidateShippingInfoAsync(details, processPaymentRequest);
        await PrepareAndValidateTotalsAsync(details, processPaymentRequest);
        //affiliate
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(details.Customer.AffiliateId);
        if (affiliate is { Active: true, Deleted: false })
            details.AffiliateId = affiliate.Id;

        //tax display type
        //TODO: this code duplicates method IWorkContext.GetTaxDisplayTypeAsync(), let's move it to a ICustomerService with "customer" parameter passing
        var taxDisplayType = _taxSettings.TaxDisplayType;
        if (_taxSettings.AllowCustomersToSelectTaxDisplayType && details.Customer.TaxDisplayTypeId.HasValue)
            taxDisplayType = (TaxDisplayType)details.Customer.TaxDisplayTypeId.Value;
        else
        {
            var defaultRoleTaxDisplayType = await _customerService.GetCustomerDefaultTaxDisplayTypeAsync(details.Customer);
            if (defaultRoleTaxDisplayType.HasValue)
                taxDisplayType = defaultRoleTaxDisplayType.Value;
        }
        details.CustomerTaxDisplayType = taxDisplayType;
        //recurring or standard shopping cart?
        details.IsRecurringShoppingCart = await _shoppingCartService.ShoppingCartIsRecurringAsync(details.Cart);
        if (!details.IsRecurringShoppingCart)
            return details;

        await PrepareAndValidateRecurringShoppingAsync(details, processPaymentRequest);
        return details;
    }

    private async Task PrepareAndValidateRecurringShoppingAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest)
    {
        var (recurringCyclesError, recurringCycleLength, recurringCyclePeriod, recurringTotalCycles) = await _shoppingCartService.GetRecurringCycleInfoAsync(details.Cart);
        if (!string.IsNullOrEmpty(recurringCyclesError))
            throw new NopException(recurringCyclesError);

        processPaymentRequest.RecurringCycleLength = recurringCycleLength;
        processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
        processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
    }

    private async Task PrepareAndValidateCustomerAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest, Currency currentCurrency)
    {
        details.Customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        if (details.Customer is null)
            throw new ArgumentException("Customer is not set");

        if (await _customerService.IsGuestAsync(details.Customer) && !_orderSettings.AnonymousCheckoutAllowed)
            throw new NopException("Anonymous checkout is not allowed");

        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(details.Customer.CurrencyId ?? 0);
        var customerCurrency = currencyTmp is { Published: true } ? currencyTmp : currentCurrency;
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
        details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
        details.CustomerLanguage = await _languageService.GetLanguageByIdAsync(details.Customer.LanguageId ?? 0);
        if (details.CustomerLanguage is null || !details.CustomerLanguage.Published)
            details.CustomerLanguage = await _workContext.GetWorkingLanguageAsync();
    }

    private async Task PrepareAndValidateCheckoutAttributesAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest, Currency currentCurrency)
    {
        details.CheckoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
        details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributesAsync(details.CheckoutAttributesXml, details.Customer);

        if (!details.Cart.Any())
            throw new NopException("Cart is empty");

        if (!await ValidateMinOrderSubtotalAmountAsync(details.Cart))
        {
            var orderAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, currentCurrency);
            throw await nopException(orderAmount, "Checkout.MinOrderSubtotalAmount");
        }

        if (!await ValidateMinOrderTotalAmountAsync(details.Cart))
        {
            var orderAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount, currentCurrency);
            throw await nopException(orderAmount, "Checkout.MinOrderTotalAmount");
        }

        async Task<NopException> nopException(decimal orderAmount, string errorResource)
        {
            var formatPrice = await _priceFormatter.FormatPriceAsync(orderAmount, true, false);
            var exception = new NopException(string.Format(await _localizationService.GetResourceAsync(errorResource), formatPrice));
            return exception;
        }
    }

    private async Task<bool> ValidateMinOrderSubtotalAmountAsync(IList<ShoppingCartItem> cart)
    {
        if (!cart.Any() || _orderSettings.MinOrderSubtotalAmount <= decimal.Zero)
            return true;
        var (_, _, subTotalWithoutDiscountBase, _, _) =
            await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart,
                _orderSettings.MinOrderSubtotalAmountIncludingTax);
        return subTotalWithoutDiscountBase >= _orderSettings.MinOrderSubtotalAmount;
    }

    private async Task<bool> ValidateMinOrderTotalAmountAsync(IList<ShoppingCartItem> cart)
    {
        if (!cart.Any() || _orderSettings.MinOrderTotalAmount <= decimal.Zero)
            return true;
        var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        return !shoppingCartTotal.shoppingCartTotal.HasValue || shoppingCartTotal.shoppingCartTotal.Value >= _orderSettings.MinOrderTotalAmount;
    }
}