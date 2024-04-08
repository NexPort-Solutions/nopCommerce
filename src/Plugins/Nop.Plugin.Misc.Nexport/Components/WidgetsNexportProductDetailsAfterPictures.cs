using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.Products;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportProductDetailsAfterPictures")]
    public class WidgetsNexportProductDetailsAfterPictures : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IStoreModelFactory _storeModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly NexportService _nexportService;

        public WidgetsNexportProductDetailsAfterPictures(
            NexportService nexportService,
            IProductModelFactory productModelFactory,
            IStoreModelFactory storeModelFactory,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ISettingService settingService,
            IWorkContext workContext,
            INexportPluginModelFactory nexportPluginModelFactory)
        {
            _nexportService = nexportService;
            _productModelFactory = productModelFactory;
            _storeModelFactory = storeModelFactory;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _settingService = settingService;
            _workContext = workContext;
            _nexportPluginModelFactory = nexportPluginModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            if (store == null)
                return Content("");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var productDetailsModel = (ProductDetailsModel?)additionalData;

            if (productDetailsModel == null)
                return Content("");

            var model = await _nexportPluginModelFactory.PrepareNexportProductRedemptionStatusesModel(customer, productDetailsModel.Id, store.Id);
            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/NexportProductDetailsAfterPictures.cshtml", model);
        }
    }
}
