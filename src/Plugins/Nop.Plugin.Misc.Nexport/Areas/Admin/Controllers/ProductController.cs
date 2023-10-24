using System.Globalization;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using static Nop.Plugin.Misc.Nexport.Defaults;
using static Nop.Plugin.Misc.Nexport.LogType;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
[Route(template: "[area]/[controller]/[action]", Order = int.MinValue)]
public class ProductController : Web.Areas.Admin.Controllers.ProductController
{
    private readonly IStoreService _store;
    private readonly Settings _settings;
    private readonly IProductMappingService _productMappingService;
    private readonly IProductMappingService _productMapping;
    private readonly IProductGroupMembershipService _productGroupMembership;
    private readonly Factories.IPluginModelFactory _model;
    private readonly IWorkContext _workContext;
    private readonly ICustomerActivityService _customerActivity;
    private readonly IProductService _product;
    private readonly ICopyProductService _copyProduct;
    private readonly IPermissionService _permission;
    private readonly ILocalizationService _localization;
    private readonly INotificationService _notification;
    private readonly ILogger _logger;

    public ProductController(
        IStoreService store,
        Settings settings,
        IProductMappingService productMapping,
        ILogger logger,
        IProductGroupMembershipService productGroupMembership,
        IAclService acl,
        IBackInStockSubscriptionService backInStockSubscription,
        ICategoryService category,
        ICopyProductService copyProduct,
        ICustomerActivityService customerActivity,
        ICustomerService customer,
        IDiscountService discount,
        IDownloadService download,
        IExportManager exportManager,
        IGenericAttributeService genericAttribute,
        IHttpClientFactory httpClientFactory,
        IImportManager importManager,
        ILanguageService language,
        ILocalizationService localization,
        ILocalizedEntityService localizedEntity,
        IManufacturerService manufacturer,
        INopFileProvider fileProvider,
        INotificationService notification,
        IPdfService pdf,
        IPermissionService permission,
        IPictureService picture,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttribute,
        Factories.IPluginModelFactory model,
        IProductModelFactory productModelFactory,
        IProductService product,
        IProductTagService productTag,
        ISettingService setting,
        IShippingService shipping,
        IShoppingCartService shoppingCart,
        ISpecificationAttributeService specificationAttribute,
        IStoreContext storeContext,
        IUrlRecordService urlRecord,
        IVideoService video,
        IWebHelper webHelper,
        IWorkContext workContext,
        VendorSettings vendorSettings)
        : base(
            acl,
            backInStockSubscription,
            category,
            copyProduct,
            customerActivity,
            customer,
            discount,
            download,
            exportManager,
            genericAttribute,
            httpClientFactory,
            importManager,
            language,
            localization,
            localizedEntity,
            manufacturer,
            fileProvider,
            notification,
            pdf,
            permission,
            picture,
            productAttributeFormatter,
            productAttributeParser,
            productAttribute,
            productModelFactory,
            product,
            productTag,
            setting,
            shipping,
            shoppingCart,
            specificationAttribute,
            storeContext,
            urlRecord,
            video,
            webHelper,
            workContext,
            vendorSettings)
    {
        _store = store;
        _productMappingService = productMapping;
        _settings = settings;
        _productMapping = productMapping;
        _model = model;
        _workContext = workContext;
        _customerActivity = customerActivity;
        _product = product;
        _copyProduct = copyProduct;
        _permission = permission;
        _localization = localization;
        _notification = notification;
        _logger = logger;
        _productGroupMembership = productGroupMembership;
    }

    [HttpGet]
    public async Task<IActionResult> SearchProducts(string term)
    {
        var stores = await _store.GetAllStoresAsync();
        var list = stores.SelectManyAwait(storeToProducts);
        var products = await list.Select(mappingToJQueryObject)
            .Where(product => product?.Label.Contains(term, StringComparison.OrdinalIgnoreCase) is true)
            .Take(10)
            .ToListAsync();
        return Json(products);

        async Task<IEnumerable<ProductMapping>> storeToProducts(Store store) => await _productMappingService.GetAllByStoreId(store.Id);
        JQueryObject<string>? mappingToJQueryObject(ProductMapping mapping)
            => mapping.DisplayName is { } name && mapping.NopProductId is { } id ? new(name, id.ToString(CultureInfo.InvariantCulture)) : null;
    }

    [Route("Admin/Product/Edit/{id}")]
    [HttpPost]
    [ActionName("Edit")]
    [FormValueRequired("syncnexportproduct")]
    public async Task<IActionResult> SyncProductWithNopProduct(ProductModel model, [FromForm(Name = "MappingId")] int mappingId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageProductMapping)
            || string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return AccessDeniedView();
        }

        var product = await _product.GetProductByIdAsync(model.Id);
        if (product?.Deleted is not false)
        {
            return RedirectToAction("List", "Product");
        }

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor is not null && product.VendorId != currentVendor.Id)
        {
            return RedirectToAction("List", "Product");
        }

        if (ModelState.IsValid)
        {
            product = model.ToEntity(product);
            await _productMapping.SyncProductAsync(mappingId, product);
            _notification.SuccessNotification("The product has been synchronized successfully with NexPort data");
        }
        return RedirectToAction("Edit", "Product", new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> CopyProduct(ProductModel model, bool copyProductMapping = false)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        {
            return AccessDeniedView();
        }

        var copyModel = model.CopyProductModel;
        try
        {
            var originalProduct = await _product.GetProductByIdAsync(copyModel.Id);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            // a vendor should have access only to his products
            if (currentVendor is not null && originalProduct.VendorId != currentVendor.Id)
            {
                return RedirectToAction("List", "Product");
            }

            var newProduct = await _copyProduct.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyMultimedia);
            if (copyProductMapping)
            {
                await _productMapping.CopyProductMappingsAsync(originalProduct, newProduct);
            }

            _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Catalog.Products.Copied"));
            return RedirectToAction("Edit", "Product", new { id = newProduct.Id });
        }
        catch (Exception exception)
        {
            _notification.ErrorNotification(exception.Message);
            return RedirectToAction("Edit", "Product", new { id = copyModel.Id });
        }
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
    {
        foreach (var mapping in await _productMapping.GetAll(eventMessage.Entity.Id))
        {
            await _productMapping.Delete(mapping);
            var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappings(mapping.Id);
            foreach (var groupMembershipMapping in groupMembershipMappings)
            {
                await _productGroupMembership.DeleteGroupMembershipMapping(groupMembershipMapping);
            }
        }
    }
}
