using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Components;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Category;

public class CategoryDetails : NopViewComponent
{
    private readonly Settings _settings;
    private readonly ICategoryService _category;
    private readonly IGenericAttributeService _genericAttribute;

    public CategoryDetails(
        Settings settings,
        ICategoryService categoryService,
        IGenericAttributeService genericAttributeService)
    {
        _settings = settings;
        _category = categoryService;
        _genericAttribute = genericAttributeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken)
            || additionalData is not Web.Areas.Admin.Models.Catalog.CategoryModel { Id: var id }
            || await _category.GetCategoryByIdAsync(id) is not { } category)
        {
            return Content(string.Empty);
        }
        var model = category.ToModel<Models.Category.CategoryModel>();
        model.LimitSingleProductPurchase = await _genericAttribute.GetAttributeAsync<bool>(category, LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY);
        model.AutoSwapProductPurchase = await _genericAttribute.GetAttributeAsync(category, AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY, defaultValue: true);
        model.AllowProductPurchaseInCategoryDuringEnrollment = await _genericAttribute.GetAttributeAsync<bool>(category, ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT);
        return View(model);
    }
}
