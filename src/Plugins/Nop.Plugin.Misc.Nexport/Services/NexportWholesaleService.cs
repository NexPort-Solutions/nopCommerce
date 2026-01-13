using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
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
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Category = Nop.Core.Domain.Catalog.Category;

namespace Nop.Plugin.Misc.Nexport.Services;

public class NexportNexportWholesaleService : INexportWholesaleService
{
    #region Fields

    private readonly IRepository<NexportFundingPool> _fundingPoolRepository;
    private readonly IRepository<NexportProductMapping> _nexportProductMappingRepository;

    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
    private readonly ISearchPluginManager _searchPluginManager;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreMappingService _storeMappingService;

    private readonly CurrencySettings _currencySettings;

    private readonly IAclService _aclService;
    private readonly IAddressService _addressService;
    private readonly IAffiliateService _affiliateService;
    private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
    private readonly ICountryService _countryService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly ICustomerActivityService _customerActivityService;
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
    private readonly IOrderProcessingService _orderProcessingService;
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
        IRepository<NexportFundingPool> fundingPoolRepository,
        IRepository<NexportProductMapping> nexportProductMappingRepository,
        IRepository<Category> categoryRepository,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
        ISearchPluginManager searchPluginManager,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService,
        IAclService aclService,
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
        IOrderProcessingService orderProcessingService,
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
        _fundingPoolRepository = fundingPoolRepository;

        _nexportProductMappingRepository = nexportProductMappingRepository;
        _categoryRepository = categoryRepository;
        _localizedPropertyRepository = localizedPropertyRepository;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
        _searchPluginManager = searchPluginManager;
        _staticCacheManager = staticCacheManager;
        _storeMappingService = storeMappingService;

        _aclService = aclService;
        _currencySettings = currencySettings;
        _addressService = addressService;
        _affiliateService = affiliateService;
        _checkoutAttributeFormatter = checkoutAttributeFormatter;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _customerActivityService = customerActivityService;
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
        _orderProcessingService = orderProcessingService;
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

    public async Task<PlaceOrderResult> PlaceWholesaleOrderAsync(ProcessPaymentRequest processPaymentRequest,
        List<ShoppingCartItem> shoppingCartItems)
    {
        if (processPaymentRequest == null)
            throw new ArgumentNullException(nameof(processPaymentRequest));

        var result = new PlaceOrderResult();

        try
        {
            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            //prepare order details
            var details = await PreparePlaceOrderForCustomerDetailsAsync(processPaymentRequest, shoppingCartItems);

            var processPaymentResult = await GetProcessPaymentResultAsync(processPaymentRequest, details);

            if (processPaymentResult == null)
                throw new NopException("processPaymentResult is not available");

            if (processPaymentResult.Success)
            {
                var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                result.PlacedOrder = order;

                var currentUser = await _workContext.GetCurrentCustomerAsync();

                var orderNote = new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"This order has been placed by user #{currentUser.Id} ({currentUser.FirstName} {currentUser.LastName} - {currentUser.Email})",
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _orderService.InsertOrderNoteAsync(orderNote);

                //move shopping cart items to order items
                await MoveTempShoppingCartItemToOrderItemsAsync(details, order);

                //discount usage history
                //await SaveDiscountUsageHistoryAsync(details, order);

                //gift card usage history
                //await SaveGiftCardUsageHistoryAsync(details, order);

                ////recurring orders
                //if (details.IsRecurringShoppingCart)
                //    await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);

                //notifications
                await SendNotificationsAndSaveNotesAsync(order);

                //reset checkout data
                await _customerService.ResetCheckoutDataAsync(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id), order);

                //raise event
                await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                //check order status
                await _orderProcessingService.CheckOrderStatusAsync(order);

                if (order.PaymentStatus == PaymentStatus.Paid)
                    await ProcessOrderPaidAsync(order);
            }
            else
                foreach (var paymentError in processPaymentResult.Errors)
                    result.AddError(string.Format(await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            result.AddError(exc.Message);
        }

        if (result.Success)
            return result;

        //log errors
        var logError = result.Errors.Aggregate("Error while placing order. ",
            (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
        var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        await _logger.ErrorAsync(logError, customer: customer);

        return result;
    }

    public virtual async Task<IPagedList<NexportFundingPool>> GetAllFundingPoolsPagination(
        string name = "", string code = "", string description = "",
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var fundingPools = await _fundingPoolRepository.GetAllPagedAsync(query =>
        {
            if (!string.IsNullOrEmpty(name))
                query = query.Where(x => x.Name.Contains(name));

            if (!string.IsNullOrEmpty(code))
                query = query.Where(x => x.Code.Contains(code));

            if (!string.IsNullOrEmpty(description))
                query = query.Where(x => x.Description.Contains(description));

            query = query.OrderByDescending(x => x.UtcDateCreated);

            return query;
        }, pageIndex, pageSize);

        return fundingPools;
    }

    public virtual async Task<IList<NexportFundingPool>> GetFundingPools()
    {
        var query = _fundingPoolRepository.Table.OrderBy(x => x.Name);

        return query.ToList();
    }

    public virtual async Task<NexportFundingPool> GetFundingPoolById(int id)
    {
        return id > 0 ? await _fundingPoolRepository.GetByIdAsync(id) : null;
    }

    public virtual async Task<IList<NexportFundingPool>> GetFundingPoolByIds(int[] fundingPoolIds)
    {
        if (fundingPoolIds == null || fundingPoolIds.Length == 0)
            return new List<NexportFundingPool>();

        var fundingPools = await _fundingPoolRepository
            .Table
            .Where(fundingPool => fundingPoolIds.Contains(fundingPool.Id))
            .ToListAsync();

        return await fundingPoolIds
            .Select(id => fundingPools.Find(x => x.Id == id))
            .Where(fundingPool => fundingPool != null)
            .ToListAsync();
    }

    public virtual async Task InsertFundingPool(NexportFundingPool nexportFundingPool)
    {
        ArgumentNullException.ThrowIfNull(nexportFundingPool);

        if (_fundingPoolRepository.Table.Any(m => m.Name == nexportFundingPool.Name))
            return;

        await _fundingPoolRepository.InsertAsync(nexportFundingPool);
    }

    public virtual async Task DeleteFundingPool(NexportFundingPool nexportFundingPool)
    {
        ArgumentNullException.ThrowIfNull(nexportFundingPool);

        await _fundingPoolRepository.DeleteAsync(nexportFundingPool);
    }

    public virtual async Task DeleteFundingPools(IList<NexportFundingPool> fundingPools)
    {
        ArgumentNullException.ThrowIfNull(fundingPools);

        foreach (var fundingPool in fundingPools)
        {
            await DeleteFundingPool(fundingPool);
        }
    }

    public virtual async Task UpdateFundingPool(NexportFundingPool nexportFundingPool)
    {
        ArgumentNullException.ThrowIfNull(nexportFundingPool);

        await _fundingPoolRepository.UpdateAsync(nexportFundingPool);
    }

    private async Task<ProcessPaymentResult> GetProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
    {
        //process payment
        ProcessPaymentResult processPaymentResult;
        //check if is payment workflow required
        if (await IsPaymentWorkflowRequiredAsync(details.Cart))
        {
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            var paymentMethod = await _paymentPluginManager
                                    .LoadPluginBySystemNameAsync(processPaymentRequest.PaymentMethodSystemName, customer, processPaymentRequest.StoreId)
                                ?? throw new NopException("Payment method couldn't be loaded");

            //ensure that payment method is active
            if (!_paymentPluginManager.IsPluginActive(paymentMethod))
                throw new NopException("Payment method is not active");

            if (details.IsRecurringShoppingCart)
            {
                //recurring cart
                processPaymentResult = (await _paymentService.GetRecurringPaymentTypeAsync(processPaymentRequest.PaymentMethodSystemName)) switch
                {
                    RecurringPaymentType.NotSupported => throw new NopException("Recurring payments are not supported by selected payment method"),
                    RecurringPaymentType.Manual or
                        RecurringPaymentType.Automatic => await _paymentService.ProcessRecurringPaymentAsync(processPaymentRequest),
                    _ => throw new NopException("Not supported recurring payment type"),
                };
            }
            else
                //standard cart
                processPaymentResult = await _paymentService.ProcessPaymentAsync(processPaymentRequest);
        }
        else
            //payment is not required
            processPaymentResult = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
        return processPaymentResult;
    }

    private async Task<bool> IsPaymentWorkflowRequiredAsync(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var result = true;

        //check whether order total equals zero
        var shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, useRewardPoints: useRewardPoints)).shoppingCartTotal;
        if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
            result = false;
        return result;
    }

    private async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
    {
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
            CustomValuesXml = processPaymentRequest.CustomValues.SerializeToXml(),
            VatNumber = details.VatNumber,
            CreatedOnUtc = DateTime.UtcNow,
            CustomOrderNumber = string.Empty
        };

        if (details.BillingAddress is null)
            throw new NopException("Billing address is not provided");

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

        order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
            string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
            order, details.RedeemedRewardPointsAmount);
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
                else
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
        await _customerService.UpdateCustomerAsync(customer);
    }

    private async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
    {
        var pIds = (await _orderService.GetOrderItemsAsync(order.Id)).Select(x => x.ProductId).ToArray();
        return await _vendorService.GetVendorsByProductIdsAsync(pIds);
    }

    protected virtual async Task SendNotificationsAndSaveNotesAsync(Order order)
    {
        //notes, messages
        await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
            ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
            : "Order placed");

        //send email notifications
        var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

        var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
            (await _pdfService.SaveOrderPdfToDiskAsync(order)) : null;
        var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
            (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;
        var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
            .SendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
        if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

        var vendors = await GetVendorsInOrderAsync(order);
        foreach (var vendor in vendors)
        {
            var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
        }

        if (order.AffiliateId == 0)
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

            //prices
            var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
            var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
            var scUnitPriceInclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, true, details.Customer);
            var scUnitPriceExclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, false, details.Customer);
            var scSubTotalInclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, true, details.Customer);
            var scSubTotalExclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, false, details.Customer);
            var discountAmountInclTax = await _taxService.GetProductPriceAsync(product, discountAmount, true, details.Customer);
            var discountAmountExclTax = await _taxService.GetProductPriceAsync(product, discountAmount, false, details.Customer);
            foreach (var disc in scDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            //attributes
            var store = await _storeService.GetStoreByIdAsync(sc.StoreId);
            var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, details.Customer, store);

            var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);

            //save order item
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

            //gift cards
            await AddGiftCardsAsync(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax.price);

            //inventory
            await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
        }
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
        public Customer Customer { get; set; }
        public Language CustomerLanguage { get; set; }
        public int AffiliateId { get; set; }
        public TaxDisplayType CustomerTaxDisplayType { get; set; }
        public string CustomerCurrencyCode { get; set; }
        public decimal CustomerCurrencyRate { get; set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public string ShippingMethodName { get; set; }
        public string ShippingRateComputationMethodSystemName { get; set; }
        public bool PickupInStore { get; set; }
        public Address PickupAddress { get; set; }
        public bool IsRecurringShoppingCart { get; set; }
        public Order InitialOrder { get; set; }
        public string CheckoutAttributeDescription { get; set; }
        public string CheckoutAttributesXml { get; set; }
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
        public string VatNumber { get; set; }
        public string TaxRates { get; set; }
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
            var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(details.Customer,
                NopCustomerDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
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
                if (details.Customer.ShippingAddressId == null)
                    throw new NopException("Shipping address is not provided");

                var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(details.Customer);

                if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                    throw new NopException("Email is not valid");

                //clone shipping address
                details.ShippingAddress = _addressService.CloneAddress(shippingAddress);

                if (await _countryService.GetCountryByAddressAsync(details.ShippingAddress) is Country shippingCountry && !shippingCountry.AllowsShipping)
                    throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
            }

            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(details.Customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, processPaymentRequest.StoreId);
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
        var (discountAmountInclTax, discountAmountExclTax, appliedDiscounts, subTotalWithoutDiscountInclTax,
                    subTotalWithoutDiscountExclTax, _, _, _) =
                await _orderTotalCalculationService.GetShoppingCartSubTotalsAsync(details.Cart);

        //sub total (incl tax)
        details.OrderSubTotalInclTax = subTotalWithoutDiscountInclTax;
        details.OrderSubTotalDiscountInclTax = discountAmountInclTax;

        //discount history
        foreach (var disc in appliedDiscounts)
            if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                details.AppliedDiscounts.Add(disc);

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
        if (_taxSettings.EuVatEnabled && details.Customer.VatNumberStatus == VatNumberStatus.Valid)
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

    private async Task<PlaceOrderContainer> PreparePlaceOrderForCustomerDetailsAsync(ProcessPaymentRequest processPaymentRequest, List<ShoppingCartItem> shoppingCart)
    {
        var details = new PlaceOrderContainer();

        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        await PrepareAndValidateCustomerAsync(details, processPaymentRequest, currentCurrency);
        await PrepareAndValidateTempShoppingCartAndCheckoutAttributesAsync(details, processPaymentRequest, currentCurrency, shoppingCart);
        await PrepareAndValidateBillingAddressAsync(details);
        await PrepareAndValidateShippingInfoAsync(details, processPaymentRequest);
        await PrepareAndValidateTotalsAsync(details, processPaymentRequest);

        //affiliate
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(details.Customer.AffiliateId);
        if (affiliate != null && affiliate.Active && !affiliate.Deleted)
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

        if (details.Customer == null)
            throw new ArgumentException("Customer is not set");

        //check whether customer is guest
        if (await _customerService.IsGuestAsync(details.Customer) && !_orderSettings.AnonymousCheckoutAllowed)
            throw new NopException("Anonymous checkout is not allowed");

        //customer currency
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(details.Customer.CurrencyId ?? 0);
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : currentCurrency;
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
        details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

        //customer language
        details.CustomerLanguage = await _languageService.GetLanguageByIdAsync(details.Customer.LanguageId ?? 0);
        if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
            details.CustomerLanguage = await _workContext.GetWorkingLanguageAsync();
    }

    /// <summary>
    /// Prepare and validate shopping cart and checkout attributes
    /// </summary>
    /// <param name="details">PlaceOrder container</param>
    /// <param name="processPaymentRequest">payment info holder</param>
    /// <param name="currentCurrency">The working currency</param>
    /// <param name="cart"></param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="NopException">Validation problems</exception>
    private async Task PrepareAndValidateTempShoppingCartAndCheckoutAttributesAsync(PlaceOrderContainer details, ProcessPaymentRequest processPaymentRequest, Currency currentCurrency,
        List<ShoppingCartItem> cart = null)
    {
        //checkout attributes
        details.CheckoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
        details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributesAsync(details.CheckoutAttributesXml, details.Customer);

        //load shopping cart
        details.Cart = cart ?? (await _shoppingCartService.GetShoppingCartAsync(details.Customer, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId)).ToList();

        if (!details.Cart.Any())
            throw new NopException("Cart is empty");

        //validate the entire shopping cart
        var warnings = await _shoppingCartService.GetShoppingCartWarningsAsync(details.Cart, details.CheckoutAttributesXml, true);
        if (warnings.Any())
            throw new NopException(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));

        //validate individual cart items
        foreach (var sci in details.Cart)
        {
            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(details.Customer,
                sci.ShoppingCartType, product, processPaymentRequest.StoreId, sci.AttributesXml,
                sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.Id);
            if (sciWarnings.Any())
                throw new NopException(sciWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
        }

        //min totals validation
        if (!await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(details.Cart))
        {
            var minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, currentCurrency);
            throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"),
                await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false)));
        }

        if (!await _orderProcessingService.ValidateMinOrderTotalAmountAsync(details.Cart))
        {
            var minOrderTotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount, currentCurrency);
            throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderTotalAmount"),
                await _priceFormatter.FormatPriceAsync(minOrderTotalAmount, true, false)));
        }
    }

    public virtual async Task<IPagedList<WholesaleOrderProduct>> SearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null,
        bool searchNexportProducts = false)
    {
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _productRepository.Table;

        if (!showHidden)
            productsQuery = productsQuery.Where(p => p.Published);

        if (!showHidden)
        {
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
        }

        productsQuery =
            from p in productsQuery
            where !p.Deleted &&
                (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                (vendorId == 0 || p.VendorId == vendorId) &&
                (
                    warehouseId == 0 ||
                    (
                        !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                            _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                    )
                ) &&
                (productType == null || p.ProductTypeId == (int)productType) &&
                (showHidden ||
                        DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                        DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                ) &&
                (priceMin == null || p.Price >= priceMin) &&
                (priceMax == null || p.Price <= priceMax)
            select p;

        if (!string.IsNullOrEmpty(keywords))
        {
            var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
            var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
            IQueryable<int> productsByKeywords;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

            if (activeSearchProvider is not null)
            {
                productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
            }
            else
            {
                productsByKeywords =
                from p in _productRepository.Table
                where p.Name.Contains(keywords) ||
                (searchDescriptions &&
                            (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                        (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                        (searchSku && p.Sku == keywords)
                select p.Id;

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from lp in _localizedPropertyRepository.Table
                        let checkName = lp.LocaleKey == nameof(Product.Name) &&
                    lp.LocaleValue.Contains(keywords)
                        let checkShortDesc = searchDescriptions &&
                    lp.LocaleKey == nameof(Product.ShortDescription) &&
                                        lp.LocaleValue.Contains(keywords)
                        where
                            lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                        select lp.EntityId);
                }
            }

            productsQuery =
                from p in productsQuery
                join pbk in productsByKeywords on p.Id equals pbk
                select p;
        }

        if (categoryIds is not null)
        {
            if (categoryIds.Contains(0))
                categoryIds.Remove(0);

            if (categoryIds.Any())
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                        categoryIds.Contains(pc.CategoryId)
                    group pc by pc.ProductId into pc
                    select new
                    {
                        ProductId = pc.Key,
                        DisplayOrder = pc.First().DisplayOrder
                    };

                productsQuery =
                    from p in productsQuery
                    join pc in productCategoryQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
        }

        if (searchNexportProducts)
        {
            productsQuery = from p in productsQuery
                            join pm in _nexportProductMappingRepository.Table on p.Id equals pm.NopProductId
                            select p;
        }

        return await productsQuery
            .OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy)
            .Select(x => new WholesaleOrderProduct(x))
            .ToPagedListAsync(pageIndex, pageSize);
    }
}