using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.Products;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportProductDetailsAfterPictures")]
    public class WidgetsNexportProductBoxAddInfoBefore : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IStoreModelFactory _storeModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly NexportService _nexportService;

        public WidgetsNexportProductBoxAddInfoBefore(
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

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if ((await _storeContext.GetCurrentStoreAsync()) == null)
                return Content("");

            //var productModel = (ProductModel)additionalData;

            //if (productModel == null)
            //    return Content("");

            var model = new NexportProductRedemptionStatusesModel {Available = 2, Awaiting = 4, Assigned = 8};

            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/NexportProductBoxAddInfoBefore.cshtml", model);
        }
    }
}
