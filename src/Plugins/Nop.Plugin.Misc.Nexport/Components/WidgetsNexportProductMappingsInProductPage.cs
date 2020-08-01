using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportProductMappingsInProductPage")]
    public class WidgetsNexportProductMappingsInProductPage : NopViewComponent
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly NexportService _nexportService;

        public WidgetsNexportProductMappingsInProductPage(
            NexportService nexportService,
            IStaticCacheManager cacheManager)
        {
            _nexportService = nexportService;
            _cacheManager = cacheManager;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var productModel = (ProductModel)additionalData;

            if (productModel == null || productModel.Id < 1)
                return Content("");

            var model = new NexportProductMappingListSearchModel() { NopProductId = productModel.Id };

            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/NexportProductMappingsInProductPage.cshtml", model);
        }
    }
}
