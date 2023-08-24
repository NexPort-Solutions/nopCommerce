using System.Diagnostics.CodeAnalysis;
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
using System.Runtime.CompilerServices;
using NexportApi.Model;
using PurchasingAgentDetails = Nop.Plugin.Misc.Nexport.Services.NexportNexportWholesaleService.WholesaleOrderDetails.PurchasingAgentDetails;
using CheckoutAttributesDetails = Nop.Plugin.Misc.Nexport.Services.NexportNexportWholesaleService.WholesaleOrderDetails.CheckoutAttributesDetails;
using TotalsDetails = Nop.Plugin.Misc.Nexport.Services.NexportNexportWholesaleService.WholesaleOrderDetails.TotalsDetails;
using Price = Nop.Plugin.Misc.Nexport.Services.NexportNexportWholesaleService.WholesaleOrderDetails.TotalsDetails.Price;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Core.Domain.Stores;
using Nop.Web.Areas.Admin.Models.Payments;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportWholesaleService
{
    Task<PlaceOrderResult> PlaceWholesaleOrderAsync(
        ProcessPaymentRequest processPaymentRequest,
        IList<ShoppingCartItem> shoppingCartItems,
        Customer customer,
        OrganizationResponseItem group,
        DateTime? redeemByUtc);
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
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IDiscountService discountService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
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

    public async Task<PlaceOrderResult> PlaceWholesaleOrderAsync(
        ProcessPaymentRequest processPaymentRequest,
        IList<ShoppingCartItem> shoppingCartItems,
        Customer customer,
        OrganizationResponseItem group,
        DateTime? redeemByUtc)
    {
        var result = new PlaceOrderResult();
        if (processPaymentRequest.OrderGuid == Guid.Empty)
        {
            result.AddError("Order GUID is not generated");
        }
        else
        {
            var details = await PreparePlaceOrderForCustomerDetailsAsync(processPaymentRequest, shoppingCartItems, group, redeemByUtc);
            var processPaymentResult = await GetProcessPaymentResultAsync(processPaymentRequest, shoppingCartItems, customer);
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
        {
            await logOrderError();
        }
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

        async Task handleSuccessfulPayment(ProcessPaymentResult processPaymentResult, WholesaleOrderDetails details)
        {
            try
            {
                var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                result.PlacedOrder = order;
                await MoveTempShoppingCartItemToOrderItemsAsync(details.PurchasingAgent.PurchasingAgent, details.Cart, details.Totals.AppliedDiscounts, order);
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

    private async Task<ProcessPaymentResult> GetProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, IList<ShoppingCartItem> cart, Customer customer)
    {
        var processPaymentResult = await IsPaymentWorkflowRequiredAsync(cart)
            ? await ProcessPaymentResultAsync(processPaymentRequest, customer)
            : new() { NewPaymentStatus = PaymentStatus.Paid };
        return processPaymentResult;
    }

    private async Task<ProcessPaymentResult> ProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, Customer customer)
    {
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(processPaymentRequest.PaymentMethodSystemName, customer, processPaymentRequest.StoreId)
            ?? throw new NopException("Payment method couldn't be loaded");
        if (!_paymentPluginManager.IsPluginActive(paymentMethod))
        {
            throw new NopException("Payment method is not active");
        }
        var result = await _paymentService.ProcessPaymentAsync(processPaymentRequest);
        return result;
    }

    private async Task<bool> IsPaymentWorkflowRequiredAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
    {
        var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, useRewardPoints: useRewardPoints);
        return shoppingCartTotal is not decimal.Zero;
    }

    private async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, WholesaleOrderDetails details)
    {
        var order = new Order
        {
            StoreId = processPaymentRequest.StoreId,
            OrderGuid = processPaymentRequest.OrderGuid,
            CustomerId = details.PurchasingAgent.PurchasingAgent.Id,
            CustomerLanguageId = details.PurchasingAgent.Language.Id,
            CustomerTaxDisplayType = details.PurchasingAgent.TaxDisplayType,
            CustomerIp = _webHelper.GetCurrentIpAddress(),
            OrderSubtotalInclTax = details.Totals.SubTotals.IncludingTax.Amount,
            OrderSubtotalExclTax = details.Totals.SubTotals.ExcludingTax.Amount,
            OrderSubTotalDiscountInclTax = details.Totals.SubTotals.IncludingTax.Discount,
            OrderSubTotalDiscountExclTax = details.Totals.SubTotals.ExcludingTax.Discount,
            OrderShippingInclTax = details.Totals.ShippingTotal.IncludingTax,
            OrderShippingExclTax = details.Totals.ShippingTotal.ExcludingTax,
            PaymentMethodAdditionalFeeInclTax = details.Totals.PaymentAdditionalFee.IncludingTax,
            PaymentMethodAdditionalFeeExclTax = details.Totals.PaymentAdditionalFee.ExcludingTax,
            TaxRates = details.Totals.Tax.Rates,
            OrderTax = details.Totals.Tax.Total,
            OrderTotal = details.Totals.Total.Amount,
            OrderDiscount = details.Totals.Total.Discount,
            CheckoutAttributeDescription = details.CheckoutAttributes.Description,
            CheckoutAttributesXml = details.CheckoutAttributes.Xml,
            CustomerCurrencyCode = details.PurchasingAgent.CurrencyCode,
            CurrencyRate = details.PurchasingAgent.CurrencyRate,
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
            PickupInStore = details.ShippingInfo?.Type is ShippingInfo.AddressType.StorePickup,
            ShippingStatus = details.ShippingInfo is null ? ShippingStatus.ShippingNotRequired : ShippingStatus.NotYetShipped,
            ShippingMethod = details.ShippingInfo?.MethodName,
            ShippingRateComputationMethodSystemName = details.ShippingInfo?.RateComputationMethodSystemName,
            CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
            VatNumber = details.Totals.VatNumber,
            CreatedOnUtc = DateTime.UtcNow,
            CustomOrderNumber = string.Empty,
        };
        await _addressService.InsertAddressAsync(details.BillingAddress);
        order.BillingAddressId = details.BillingAddress?.Id ?? throw new NopException("Billing address is not provided");
        switch (details.ShippingInfo?.Type)
        {
            case ShippingInfo.AddressType.StorePickup:
                await _addressService.InsertAddressAsync(details.ShippingInfo.Address);
                order.PickupAddressId = details.ShippingInfo.Address.Id;
                break;
            case ShippingInfo.AddressType.CustomerAddress:
                await _addressService.InsertAddressAsync(details.ShippingInfo.Address);
                order.ShippingAddressId = details.ShippingInfo.Address.Id;
                break;
            case null:
                break;
            default:
                throw new NotImplementedException();
        }
        await _orderService.InsertOrderAsync(order);
        await _genericAttributeService.SaveAttributeAsync(order, "GroupMembership", details.MembershipGroup.OrgId);
        await _genericAttributeService.SaveAttributeAsync(order, nameof(details.RedeemByUtc), details.RedeemByUtc);
        //generate and set custom order number
        order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
        await _orderService.UpdateOrderAsync(order);
        //reward points history
        if (details.Totals.Redeemed.RewardPointsAmount <= decimal.Zero)
        {
            return order;
        }
        var message = string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber);
        order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.PurchasingAgent.PurchasingAgent, -details.Totals.Redeemed.RedeemedRewardPoints, order.StoreId, message, order, details.Totals.Redeemed.RewardPointsAmount);
        await _customerService.UpdateCustomerAsync(details.PurchasingAgent.PurchasingAgent);
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
            {
                await AddOrderNoteAsync(order, buildNote("customer", orderPaidCustomerNotificationQueuedEmailIds));
            }
            var orderPaidStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPaidStoreOwnerNotificationQueuedEmailIds.Any())
            {
                await AddOrderNoteAsync(order, buildNote("store owner", orderPaidStoreOwnerNotificationQueuedEmailIds));
            }
            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPaidVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidVendorNotificationQueuedEmailIds.Any())
                {
                    await AddOrderNoteAsync(order, buildNote("vendor", orderPaidVendorNotificationQueuedEmailIds));
                }
            }
            if (order.AffiliateId != 0)
            {
                var orderPaidAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidAffiliateNotificationQueuedEmailIds.Any())
                {
                    await AddOrderNoteAsync(order, buildNote("affiliate", orderPaidAffiliateNotificationQueuedEmailIds));
                }
            }
        }

        //customer roles with "purchased with product" specified
        await ProcessCustomerRolesWithPurchasedProductSpecifiedAsync(order, true);

        static string buildNote(string emailToEntityName, IEnumerable<int> emailIds)
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
        {
            return;
        }
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        foreach (var customerRole in customerRoles)
        {
            if (!await _customerService.IsInCustomerRoleAsync(customer, customerRole.SystemName))
            {
                if (add)
                {
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                }
                else if (!add)
                {
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
                }
            }
        }
        await _customerService.UpdateCustomerAsync(customer);
    }

    private async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
    {
        var pIds = (await _orderService.GetOrderItemsAsync(order.Id)).Select(x => x.ProductId).ToArray();
        return await _vendorService.GetVendorsByProductIdsAsync(pIds);
    }

    private async Task MoveTempShoppingCartItemToOrderItemsAsync(Customer customer, IList<ShoppingCartItem> cart, List<Discount> discounts, Order order)
    {
        foreach (var sc in cart)
        {
            var product = await _productService.GetProductByIdAsync(sc.ProductId);
            var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
            var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
            var (scUnitPriceInclTax, _) = await _taxService.GetProductPriceAsync(product, scUnitPrice, true, customer);
            var (scUnitPriceExclTax, _) = await _taxService.GetProductPriceAsync(product, scUnitPrice, false, customer);
            var (scSubTotalInclTax, _) = await _taxService.GetProductPriceAsync(product, scSubTotal, true, customer);
            var (scSubTotalExclTax, _) = await _taxService.GetProductPriceAsync(product, scSubTotal, false, customer);
            var (discountAmountInclTax, _) = await _taxService.GetProductPriceAsync(product, discountAmount, true, customer);
            var (discountAmountExclTax, _) = await _taxService.GetProductPriceAsync(product, discountAmount, false, customer);
            discounts.AddRange(scDiscounts.Where(disc => !_discountService.ContainsDiscount(discounts, disc)));
            var store = await _storeService.GetStoreByIdAsync(sc.StoreId);
            var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, customer, store);
            var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);
            var orderItem = new OrderItem
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPriceInclTax = scUnitPriceInclTax,
                UnitPriceExclTax = scUnitPriceExclTax,
                PriceInclTax = scSubTotalInclTax,
                PriceExclTax = scSubTotalExclTax,
                OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, sc.AttributesXml),
                AttributeDescription = attributeDescription,
                AttributesXml = sc.AttributesXml,
                Quantity = sc.Quantity,
                DiscountAmountInclTax = discountAmountInclTax,
                DiscountAmountExclTax = discountAmountExclTax,
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = 0,
                ItemWeight = itemWeight,
                RentalStartDateUtc = sc.RentalStartDateUtc,
                RentalEndDateUtc = sc.RentalEndDateUtc
            };
            await _orderService.InsertOrderItemAsync(orderItem);
            await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml, string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
        }
        //clear shopping cart
        await Task.WhenAll(cart.Select(sci => _shoppingCartService.DeleteShoppingCartItemAsync(sci, false)));
    }

    private async Task<Address> PrepareAndValidateBillingAddressAsync(Customer customer)
    {
        if (customer.BillingAddressId is null)
        {
            throw new NopException("Billing address is not provided");
        }
        var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);
        if (!CommonHelper.IsValidEmail(billingAddress?.Email))
        {
            throw new NopException("Email is not valid");
        }
        var clonedBillingAddress = _addressService.CloneAddress(billingAddress);
        if (await _countryService.GetCountryByAddressAsync(clonedBillingAddress) is { AllowsBilling: false } billingCountry)
        {
            throw new NopException($"Country '{billingCountry.Name}' is not allowed for billing");
        }
        return clonedBillingAddress;
    }

    internal record ShippingInfo(Address Address, string MethodName, string RateComputationMethodSystemName, ShippingInfo.AddressType Type)
    {
        internal enum AddressType
        {
            StorePickup = 0,
            CustomerAddress = 1
        }
    };

    private async Task<ShippingInfo?> PrepareAndValidateShippingInfoAsync(Customer customer, IList<ShoppingCartItem> cart, ProcessPaymentRequest processPaymentRequest)
    {
        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart)) return null;
        var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, processPaymentRequest.StoreId);
        var (name, rateComputationSystemName) = (shippingOption.Name, shippingOption.ShippingRateComputationMethodSystemName);
        var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
        if (_shippingSettings.AllowPickupInStore && pickupPoint is not null)
        {
            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);
            var pickupAddress = new Address
            {
                Address1 = pickupPoint.Address,
                City = pickupPoint.City,
                County = pickupPoint.County,
                CountryId = country?.Id,
                StateProvinceId = state?.Id,
                ZipPostalCode = pickupPoint.ZipPostalCode,
                CreatedOnUtc = DateTime.UtcNow
            };
            return new(pickupAddress, name, rateComputationSystemName, ShippingInfo.AddressType.StorePickup);
        }
        if (customer.ShippingAddressId is null)
        {
            throw new NopException("Shipping address is not provided");
        }
        var address = await _customerService.GetCustomerShippingAddressAsync(customer);
        if (!CommonHelper.IsValidEmail(address?.Email))
        {
            throw new NopException("Email is not valid");
        }
        var clonedAddress = _addressService.CloneAddress(address);
        if (await _countryService.GetCountryByAddressAsync(clonedAddress) is { AllowsShipping: false } shippingCountry)
        {
            throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
        }
        return new(clonedAddress, name, rateComputationSystemName, ShippingInfo.AddressType.CustomerAddress);
    }

    private async Task<TotalsDetails> PrepareAndValidateTotalsAsync(Customer customer, IList<ShoppingCartItem> cart, ProcessPaymentRequest processPaymentRequest)
    {
        async Task<(TotalsDetails.Taxed<Price> taxedPrice, List<Discount> discounts)> getSubtotals(IList<ShoppingCartItem> cart)
        {
            var (discountAmountInclTax, discountAmountExcludingTax, list, subtotalWithoutDiscountIncludingTax, subtotalWithoutDiscountExclTax, _, _, _) =
                await _orderTotalCalculationService.GetShoppingCartSubTotalsAsync(cart);
            var taxed = new TotalsDetails.Taxed<Price>(new(subtotalWithoutDiscountIncludingTax, discountAmountInclTax), new(subtotalWithoutDiscountExclTax, discountAmountExcludingTax));
            return (taxed, list);
        }

        var (subtotals, appliedDiscounts) = await getSubtotals(cart);
        var (orderShippingTotalIncludingTax, orderShippingTotalExcludingTax, _, shippingTotalDiscounts) = await _orderTotalCalculationService.GetShoppingCartShippingTotalsAsync(cart);
        var paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, processPaymentRequest.PaymentMethodSystemName);
        var includingTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, true, customer)).price;
        var excludingTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, false, customer)).price;
        var (orderTaxTotal, taxRatesDictionary) = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
        var taxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) => $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");
        var customerVatNumber = _taxSettings.EuVatEnabled && customer.VatNumberStatus is VatNumberStatus.Valid ? customer.VatNumber : default;
        var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        // `Nop.Services.Orders.OrderTotalCalculationService.GetShoppingCartTotalAsync` never returns `default` for `shoppingCartTotal`.
        processPaymentRequest.OrderTotal = ExpectValue(shoppingCartTotal.shoppingCartTotal!);
        bool notContained(Discount disc) => !_discountService.ContainsDiscount(appliedDiscounts, disc);
        appliedDiscounts.AddRange(shippingTotalDiscounts.Concat(shoppingCartTotal.appliedDiscounts).Where(notContained));
        return new()
        {
            SubTotals = subtotals,
            // `Nop.Services.Orders.OrderTotalCalculationService.GetShoppingCartShippingTotalsAsync` never returns `default` for `orderShippingTotalInclTax` or `orderShippingTotalExclTax`.
            ShippingTotal = new(ExpectValue(orderShippingTotalIncludingTax!), ExpectValue(orderShippingTotalExcludingTax!)),
            AppliedDiscounts = appliedDiscounts,
            PaymentAdditionalFee = new(includingTax, excludingTax),
            Tax = (orderTaxTotal, taxRates),
            VatNumber = customerVatNumber,
            Total = new(processPaymentRequest.OrderTotal, shoppingCartTotal.discountAmount),
            Redeemed = (shoppingCartTotal.redeemedRewardPoints, shoppingCartTotal.redeemedRewardPointsAmount, shoppingCartTotal.appliedGiftCards),
        };
    }

    private async Task<WholesaleOrderDetails> PreparePlaceOrderForCustomerDetailsAsync(ProcessPaymentRequest processPaymentRequest, IList<ShoppingCartItem> shoppingCartItems, OrganizationResponseItem group, DateTime? redeemByUtc)
    {
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        await ValidateCartAsync(shoppingCartItems, currentCurrency);
        var purchasingAgent = await PrepareAndValidateCustomerAsync(processPaymentRequest, currentCurrency);
        var details = new WholesaleOrderDetails
        {
            Cart = shoppingCartItems,
            PurchasingAgent = purchasingAgent,
            CheckoutAttributes = await PrepareCheckoutAttributesAsync(purchasingAgent.PurchasingAgent, processPaymentRequest),
            BillingAddress = await PrepareAndValidateBillingAddressAsync(purchasingAgent.PurchasingAgent),
            ShippingInfo = await PrepareAndValidateShippingInfoAsync(purchasingAgent.PurchasingAgent, shoppingCartItems, processPaymentRequest),
            Totals = await PrepareAndValidateTotalsAsync(purchasingAgent.PurchasingAgent, shoppingCartItems, processPaymentRequest),
            AffiliateId = await _affiliateService.GetAffiliateByIdAsync(purchasingAgent.PurchasingAgent.AffiliateId) is { Active: true, Deleted: false } affiliate ? affiliate.Id : default,
            MembershipGroup = group,
            RedeemByUtc = redeemByUtc
        };
        return details;
    }

    private async Task ValidateCartAsync(IList<ShoppingCartItem> cart, Currency currentCurrency)
    {
        if (!cart.Any())
        {
            throw new NopException("Cart is empty");
        }
        if (!await ValidateMinOrderSubtotalAmountAsync(cart))
        {
            var orderAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, currentCurrency);
            throw await nopException(orderAmount, "Checkout.MinOrderSubtotalAmount");
        }
        if (!await ValidateMinOrderTotalAmountAsync(cart))
        {
            var orderAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount, currentCurrency);
            throw await nopException(orderAmount, "Checkout.MinOrderTotalAmount");
        }

        async Task<NopException> nopException(decimal orderAmount, string errorResource)
        {
            var formatPrice = await _priceFormatter.FormatPriceAsync(orderAmount, true, false);
            var message = string.Format(await _localizationService.GetResourceAsync(errorResource), formatPrice);
            var exception = new NopException(message);
            return exception;
        }
    }

    private async Task<TaxDisplayType> TaxDisplayTypeAsync(Customer customer)
    {
        var customerTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType ? (TaxDisplayType?)customer.TaxDisplayTypeId : null;
        return customerTaxDisplayType
            ?? await _customerService.GetCustomerDefaultTaxDisplayTypeAsync(customer)
            ?? _taxSettings.TaxDisplayType;
    }

    private async Task<PurchasingAgentDetails> PrepareAndValidateCustomerAsync(ProcessPaymentRequest processPaymentRequest, Currency currentCurrency)
    {
        var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
        {
            throw new NopException("Anonymous checkout is not allowed");
        }
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId ?? 0);
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var customerCurrency = currencyTmp is { Published: true } ? currencyTmp : currentCurrency;
        var language = await _languageService.GetLanguageByIdAsync(customer.LanguageId ?? 0);
        if (language?.Published is not true)
        {
            language = await _workContext.GetWorkingLanguageAsync();
        }
        var taxDisplayType = await TaxDisplayTypeAsync(customer);
        return new(customer, customerCurrency.CurrencyCode, customerCurrency.Rate / primaryStoreCurrency.Rate, language, taxDisplayType);
    }

    private async Task<CheckoutAttributesDetails> PrepareCheckoutAttributesAsync(Customer purchasingAgent, ProcessPaymentRequest processPaymentRequest)
    {
        var xml = await _genericAttributeService.GetAttributeAsync<string>(purchasingAgent, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
        var description = await _checkoutAttributeFormatter.FormatAttributesAsync(xml, purchasingAgent);
        return new(xml, description);
    }

    private async Task<bool> ValidateMinOrderSubtotalAmountAsync(IList<ShoppingCartItem> cart)
    {
        if (!cart.Any() || _orderSettings.MinOrderSubtotalAmount <= decimal.Zero)
        {
            return true;
        }
        var (_, _, subtotalWithoutDiscountBase, _, _) =
            await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart,
                _orderSettings.MinOrderSubtotalAmountIncludingTax);
        return subtotalWithoutDiscountBase >= _orderSettings.MinOrderSubtotalAmount;
    }

    private async Task<bool> ValidateMinOrderTotalAmountAsync(IList<ShoppingCartItem> cart)
    {
        if (!cart.Any() || _orderSettings.MinOrderTotalAmount <= decimal.Zero)
        {
            return true;
        }
        var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        return shoppingCartTotal is null || shoppingCartTotal >= _orderSettings.MinOrderTotalAmount;
    }

    private static T ExpectValue<T>([DisallowNull] T? nullableT, [CallerArgumentExpression(nameof(nullableT))] string? callerExpression = null)
        where T : struct
        => nullableT ?? throw new NopException($"{callerExpression} couldn't be calculated");

    internal class WholesaleOrderDetails
    {
        internal required PurchasingAgentDetails PurchasingAgent { get; init; }
        internal required CheckoutAttributesDetails CheckoutAttributes { get; init; }
        internal required TotalsDetails Totals { get; init; }
        internal required ShippingInfo? ShippingInfo { get; init; }
        internal required int AffiliateId { get; init; }
        internal required Address? BillingAddress { get; set; }
        internal required IList<ShoppingCartItem> Cart { get; init; }
        public required OrganizationResponseItem MembershipGroup { get; init; }
        public required DateTime? RedeemByUtc { get; init; }

        internal record PurchasingAgentDetails(Customer PurchasingAgent, string? CurrencyCode, decimal CurrencyRate, Language Language, TaxDisplayType TaxDisplayType);

        internal record CheckoutAttributesDetails(string Xml, string Description);

        internal record TotalsDetails
        {
            internal required Taxed<Price> SubTotals { get; init; }
            internal required Taxed<decimal> ShippingTotal { get; init; }
            internal required Taxed<decimal> PaymentAdditionalFee { get; set; }
            internal required Price Total { get; init; }
            internal required List<Discount> AppliedDiscounts { get; init; }
            internal required (decimal Total, string Rates) Tax { get; set; }
            internal required string? VatNumber { get; init; }
            internal required (int RedeemedRewardPoints, decimal RewardPointsAmount, IList<AppliedGiftCard> AppliedGiftCards) Redeemed { get; init; }

            internal record Price(decimal Amount, decimal Discount);

            internal record Taxed<T>(T IncludingTax, T ExcludingTax);
        }
    }
}
