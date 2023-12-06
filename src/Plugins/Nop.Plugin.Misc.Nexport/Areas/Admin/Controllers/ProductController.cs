using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[Route("[area]/[controller]/[action]", Order = 100)]
public class ProductController : Web.Areas.Admin.Controllers.ProductController
{
    private readonly IPermissionService _permissionService;
    private readonly IProductModelFactory _productModelFactory;

    #region Constructor
    public ProductController(
        IAclService aclService,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        ICategoryService categoryService,
        ICopyProductService copyProductService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IGenericAttributeService genericAttributeService,
        IHttpClientFactory httpClientFactory,
        IImportManager importManager,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IManufacturerService manufacturerService,
        INopFileProvider fileProvider,
        INotificationService notificationService,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISettingService settingService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        VendorSettings vendorSettings)
            : base(
                aclService,
                backInStockSubscriptionService,
                categoryService,
                copyProductService,
                customerActivityService,
                customerService,
                discountService,
                downloadService,
                exportManager,
                genericAttributeService,
                httpClientFactory,
                importManager,
                languageService,
                localizationService,
                localizedEntityService,
                manufacturerService,
                fileProvider,
                notificationService,
                pdfService,
                permissionService,
                pictureService,
                productAttributeFormatter,
                productAttributeParser,
                productAttributeService,
                productModelFactory,
                productService,
                productTagService,
                settingService,
                shippingService,
                shoppingCartService,
                specificationAttributeService,
                storeContext,
                urlRecordService,
                videoService,
                webHelper,
                workContext,
                vendorSettings)
    {
        _permissionService = permissionService;
        _productModelFactory = productModelFactory;
    }

    #endregion Constructor

    public override async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        {
            return AccessDeniedView();
        }
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());
        return View(model);
    }
}
