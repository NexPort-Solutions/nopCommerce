using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportProductMappingsInProductPage")]
    public class WidgetsNexportProductMappingsInProductPage : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IStoreModelFactory _storeModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly NexportService _nexportService;

        public WidgetsNexportProductMappingsInProductPage(
            NexportService nexportService,
            IProductModelFactory productModelFactory,
            IStoreModelFactory storeModelFactory,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ISettingService settingService)
        {
            _nexportService = nexportService;
            _productModelFactory = productModelFactory;
            _storeModelFactory = storeModelFactory;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (_storeContext.CurrentStore == null)
                return Content("");

            var productModel = (ProductModel)additionalData;

            var model = _nexportService.GetProductMappingByNopProductId(productModel.Id);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/NexportProductMappingsInProductPage.cshtml", model);
        }
    }
}
