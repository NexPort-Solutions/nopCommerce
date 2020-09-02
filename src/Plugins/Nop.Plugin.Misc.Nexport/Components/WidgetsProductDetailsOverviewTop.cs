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

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (_storeContext.CurrentStore == null)
                return Content("");

            var productDetailsModel = (ProductDetailsModel)additionalData;

            if (productDetailsModel == null)
                return Content("");

            var model = _nexportService.GetProductMappingByNopProductId(productDetailsModel.Id, _storeContext.CurrentStore.Id) ??
                        _nexportService.GetProductMappingByNopProductId(productDetailsModel.Id);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Product/WidgetsProductDetailsOverviewTop.cshtml", model);
        }
    }
}
