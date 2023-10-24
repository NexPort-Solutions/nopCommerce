using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using NexportApi.Model;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ICustomerPurchasingService
{
    Task<bool> CanPurchaseDifferentProductInCategoryAsync(Product product, Customer customer, int storeId);
    Task<bool> CanPurchaseProductAsync(Product product, Customer customer);
    Task<bool> ExceedExtensionPurchaseLimitAsync(Customer customer, ProductMapping productMapping);
    Task<(ShoppingCartItem, Category)?> CanPurchaseProductInCategoryAsync(Product product, int storeId);
}

public class CustomerPurchasingService : ICustomerPurchasingService
{
    private readonly IProductMappingService _productMappingService;
    private readonly IProductService _product;
    private readonly ICategoryService _category;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IStoreContext _storeContext;
    private readonly INexportService _nexportService;
    private readonly IUserMappingService _userMapping;
    private readonly IOrderService _order;
    private readonly IInvoiceService _invoiceService;
    private readonly IShoppingCartService _shoppingCart;
    private readonly IWorkContext _workContext;

    public CustomerPurchasingService(
        IProductMappingService productMappingService,
        IProductService product,
        ICategoryService category,
        IGenericAttributeService genericAttribute,
        IStoreContext storeContext,
        INexportService nexportService,
        IInvoiceService invoiceService,
        IOrderService order,
        IUserMappingService userMapping,
        IWorkContext workContext,
        IShoppingCartService shoppingCart)
    {
        _productMappingService = productMappingService;
        _product = product;
        _category = category;
        _genericAttribute = genericAttribute;
        _storeContext = storeContext;
        _nexportService = nexportService;
        _invoiceService = invoiceService;
        _order = order;
        _userMapping = userMapping;
        _workContext = workContext;
        _shoppingCart = shoppingCart;
    }

    public async Task<bool> CanPurchaseDifferentProductInCategoryAsync(Product product, Customer customer, int storeId)
    {
        foreach (var categoryId in (await _category.GetProductCategoriesByProductIdAsync(product.Id, true)).Select(category => category.CategoryId))
        {
            if (await _category.GetCategoryByIdAsync(categoryId) is not { } category
                || await _genericAttribute.GetAttributeAsync(category, Defaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT, defaultValue: true))
            {
                continue;
            }
            foreach (var productId in (await _category.GetProductCategoriesByCategoryIdAsync(categoryId, showHidden: true)).Select(category => category.ProductId))
            {
                if ((await _productMappingService.GetByNopProductId(productId, storeId)
                    ?? await _productMappingService.GetByNopProductId(productId)) is null
                    || !(await _product.GetProductByIdAsync(productId) is { } otherProduct))
                {
                    continue;
                }
                if (await _nexportService.VerifyEnrollmentStatusAsync(otherProduct, customer, storeId) is { } status
                    && status.EnrollmentExpirationDate >= DateTime.UtcNow
                    && status.Phase is Enums.PhaseEnum.NotStarted or Enums.PhaseEnum.InProgress)
                {
                    // Customer is not allowed to purchase this product
                    // since there is an existing enrollment from a different product withthis category
                    // that has not been expired and that enrollment is currently either the Not Started or In Progress phase.
                    return false;
                }
            }
        }
        return true;
    }

    public async Task<bool> CanPurchaseProductAsync(Product product, Customer customer)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        if ((await _productMappingService.GetByNopProductId(product.Id, store.Id) ?? await _productMappingService.GetByNopProductId(product.Id)) is not { } mapping)
        {
            return true;
        }
        if (await _nexportService.VerifyEnrollmentStatusAsync(product, customer, store.Id) is not { EnrollmentExpirationDate: var expiration, Phase: var phase, Result: var result })
        {
            return !mapping.IsExtensionProduct && await CanPurchaseDifferentProductInCategoryAsync(product, customer, store.Id);
        }
        return (phase, result) switch
        {
            (Enums.PhaseEnum.Finished, Enums.ResultEnum.Failing) => await DoesStoreAllowPurchasingAfterFailure(store),
            (Enums.PhaseEnum.Finished, Enums.ResultEnum.Passing) => await DoesStoreAllowPurchasingAfterPassing(store),
            (Enums.PhaseEnum.InProgress or Enums.PhaseEnum.NotStarted, _) => await MayRenew(customer, mapping, expiration),
            _ => true,
        };
    }

    public async Task<(ShoppingCartItem, Category)?> CanPurchaseProductInCategoryAsync(Product product, int storeId)
    {
        var productCategories = await _category.GetProductCategoriesByProductIdAsync(product.Id, true);
        var shoppingCartItemsExceptCurrentProduct = await (await _shoppingCart
            .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, storeId))
            .Where(cartItem => cartItem.ProductId != product.Id)
            .ToListAsync();
        foreach (var productCategory in productCategories)
        {
            var category = await _category.GetCategoryByIdAsync(productCategory.CategoryId);
            if (category is null)
            {
                continue;
            }
            var limitSinglePurchase = await _genericAttribute.GetAttributeAsync<bool>(category, Defaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY);
            if (limitSinglePurchase)
            {
                var productInSameCategory = await shoppingCartItemsExceptCurrentProduct.FirstOrDefaultAwaitAsync(async itemProduct =>
                    (await _category
                        .GetProductCategoriesByProductIdAsync(itemProduct.ProductId, true))
                        .Any(category => category.CategoryId == productCategory.CategoryId));
                return (productInSameCategory, category);
            }
        }
        return null;
    }

    private Task<bool> DoesStoreAllowPurchasingAfterPassing(Store store)
        => _genericAttribute.GetAttributeAsync<bool>(store, Defaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);

    private Task<bool> DoesStoreAllowPurchasingAfterFailure(Store store)
        => _genericAttribute.GetAttributeAsync<bool>(store, Defaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);

    // Customer is allowed to purchase this product under one of these scenarios:
    // A - The product allows extension, the purchase limit has not exceed yet, and the enrollment has been expired.
    // B - The product allows extension, the purchase limit has not exceed yet, the enrollment has not yet expired, and it is withthe renewal window time-frame.
    private async Task<bool> MayRenew(Customer customer, ProductMapping mapping, DateTime? expiration)
    {
        if (!await ExtensionIsAllowed(customer, expiration, mapping))
        {
            return false;
        }
        else if (expiration <= DateTime.UtcNow)
        {
            return true;
        }
        else if (TimeSpan.TryParse(mapping.RenewalWindow, out var renewalWindow))
        {
            return expiration - renewalWindow <= DateTime.UtcNow;
        }
        return false;
    }

    private async Task<bool> ExtensionIsAllowed(Customer customer, DateTime? enrollementExpirationDate, ProductMapping mapping)
        => mapping.AllowExtension && !await ExceedExtensionPurchaseLimitAsync(customer, mapping) && enrollementExpirationDate is not null;

    public async Task<bool> ExceedExtensionPurchaseLimitAsync(Customer customer, ProductMapping productMapping)
    {
        var userMapping = await _userMapping.FindByCustomerId(customer.Id);
        if (productMapping.ExtensionPurchaseLimit is null || userMapping is null)
        {
            return false;
        }
        var previousInvoiceItems = await _invoiceService.GetOrderInvoiceItems(userMapping.UserId);
        var previousExtensionCount = await previousInvoiceItems
            .SelectAwait(async invoiceItem => await _order.GetOrderItemByIdAsync(invoiceItem.OrderItemId))
            .CountAsync(orderItem => orderItem.ProductId == productMapping.NopProductId)
            - 1;
        return previousExtensionCount > productMapping.ExtensionPurchaseLimit;
    }
}
