using Nop.Core;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Nop.Services.ExportImport;
using Nop.Services.Media;
using Nop.Core.Caching;
using Nop.Services.Stores;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
[Route(template: "[area]/[controller]/[action]", Order = int.MinValue)]
public class CategoryController :
    Web.Areas.Admin.Controllers.CategoryController,
    IConsumer<EntityInsertedEvent<Category>>,
    IConsumer<EntityDeletedEvent<Category>>
{
    private readonly ICategoryService _category;
    private readonly IPermissionService _permission;
    private readonly ILocalizationService _localization;
    private readonly INotificationService _notification;
    private readonly IGenericAttributeService _genericAttribute;
    private static readonly string s_this = ViewUtilities.GetControllerName<CategoryController>();

    public CategoryController(
        IAclService aclService,
        ICategoryModelFactory categoryModelFactory,
        ICategoryService categoryService,
        ICustomerActivityService customerActivityService,
        Nop.Services.Customers.ICustomerService customerService,
        IDiscountService discountService,
        IExportManager exportManager,
        IImportManager importManager,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductService productService,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        ICategoryService category,
        IPermissionService permission,
        ILocalizationService localization,
        INotificationService notification,
        IGenericAttributeService genericAttribute)
            : base(
                aclService,
                categoryModelFactory,
                categoryService,
                customerActivityService,
                customerService,
                discountService,
                exportManager,
                importManager,
                localizationService,
                localizedEntityService,
                notificationService,
                permissionService,
                pictureService,
                productService,
                staticCacheManager,
                storeMappingService,
                storeService,
                urlRecordService,
                workContext)
    {
        _category = category;
        _permission = permission;
        _localization = localization;
        _notification = notification;
        _genericAttribute = genericAttribute;
    }

    [Route("/Admin/Category/Edit/{id}")]
    [HttpPost]
    [ActionName("Edit")]
    [FormValueRequired("savenexportcategoryoptions")]
    public async Task<IActionResult> SaveCategoryOptions(Nexport.Models.Category.CategoryModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
        {
            return AccessDeniedView();
        }
        if (await _category.GetCategoryByIdAsync(model.Id) is not { } category)
        {
            return RedirectToAction(nameof(List), s_this);
        }
        await _genericAttribute.SaveAttributeAsync(category, LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY, model.LimitSingleProductPurchase);
        await _genericAttribute.SaveAttributeAsync(category, AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY, model.AutoSwapProductPurchase);
        await _genericAttribute.SaveAttributeAsync(category, ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT, model.AllowProductPurchaseInCategoryDuringEnrollment);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Catalog.Categories.Updated"));
        return RedirectToAction(nameof(Edit), s_this, new { id = category.Id });
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
    {
        var category = eventMessage.Entity;
        await _genericAttribute.SaveAttributeAsync(category, LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY, false);
        await _genericAttribute.SaveAttributeAsync(category, AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY, true);
        await _genericAttribute.SaveAttributeAsync(category, ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT, true);
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
    {
        var deletedCategory = eventMessage.Entity;
        var categoryAttributes = await _genericAttribute.GetAttributesForEntityAsync(deletedCategory.Id, "Category");
        await _genericAttribute.DeleteAttributesAsync(categoryAttributes);
    }
}
