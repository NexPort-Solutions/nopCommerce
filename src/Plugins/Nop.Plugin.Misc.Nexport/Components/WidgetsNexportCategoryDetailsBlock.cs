using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportCategoryDetailsBlock")]
    public class WidgetsNexportCategoryDetailsBlock : NopViewComponent
    {
        private readonly NexportSettings _nexportSettings;
        private readonly NexportService _nexportService;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IGenericAttributeService _genericAttributeService;

        public WidgetsNexportCategoryDetailsBlock(
            NexportSettings nexportSettings,
            NexportService nexportService,
            INexportPluginModelFactory nexportPluginModelFactory,
            ICategoryService categoryService,
            IGenericAttributeService genericAttributeService)
        {
            _nexportSettings = nexportSettings;
            _nexportService = nexportService;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _categoryService = categoryService;
            _genericAttributeService = genericAttributeService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return Content("");

            var categoryModel = (CategoryModel) additionalData;

            var category = _categoryService.GetCategoryById(categoryModel.Id);

            if (category == null)
                return Content("");

            var model = category.ToModel<NexportCategoryModel>();

            model.LimitSingleProductPurchase = _genericAttributeService.GetAttribute<bool>(category,
                NexportDefaults.LIMIT_SINGLE_PRODUCT_PURCHASE_IN_CATEGORY);

            model.AutoSwapProductPurchase = _genericAttributeService.GetAttribute(category,
                NexportDefaults.AUTO_SWAP_PRODUCT_PURCHASE_IN_CATEGORY, defaultValue: true);

            model.AllowProductPurchaseInCategoryDuringEnrollment = _genericAttributeService.GetAttribute<bool>(category,
                NexportDefaults.ALLOW_PRODUCT_PURCHASE_IN_CATEGORY_DURING_ENROLLMENT);

            return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Widget/Category/NexportCategoryDetails.cshtml", model);
        }
    }
}
