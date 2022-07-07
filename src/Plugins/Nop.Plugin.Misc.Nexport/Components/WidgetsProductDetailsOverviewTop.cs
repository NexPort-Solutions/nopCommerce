using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsProductDetailsOverviewTop")]
    public class WidgetsProductDetailsOverviewTop : NopViewComponent
    {
        private readonly NexportService _nexportService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public WidgetsProductDetailsOverviewTop(
            NexportService nexportService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _nexportService = nexportService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if ((await _storeContext.GetCurrentStoreAsync()) == null)
                return Content("");

            var productDetailsModel = (ProductDetailsModel)additionalData;

            if (productDetailsModel == null)
                return Content("");

            var model = await _nexportService.GetProductMappingByNopProductId(productDetailsModel.Id,
                            (await _storeContext.GetCurrentStoreAsync()).Id) ??
                        await _nexportService.GetProductMappingByNopProductId(productDetailsModel.Id);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/WidgetsProductDetailsOverviewTop.cshtml", model);
        }
    }
}
