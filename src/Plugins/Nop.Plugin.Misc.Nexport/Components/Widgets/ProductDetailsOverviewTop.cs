using Nop.Core;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets;

[ViewComponent(Name = "ProductDetailsOverviewTop")]
public class ProductDetailsOverviewTop : NopViewComponent
{
    private readonly IProductMappingService _productMappingService;
    private readonly IStoreContext _storeContext;

    public ProductDetailsOverviewTop(
        IProductMappingService productMappingService,
        IStoreContext storeContext)
    {
        _productMappingService = productMappingService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (await _storeContext.GetCurrentStoreAsync() is null)
        {
            return Content(string.Empty);
        }

        var productDetailsModel = (ProductDetailsModel)additionalData;
        if (productDetailsModel is null)
        {
            return Content(string.Empty);
        }

        var model = await _productMappingService.GetByNopProductId(
            productDetailsModel.Id,
            (await _storeContext.GetCurrentStoreAsync()).Id)
            ?? await _productMappingService.GetByNopProductId(productDetailsModel.Id);
        return View("~/Plugins/Misc.Nexport/Views/Widget/Product/ProductDetailsOverviewTop.cshtml", model);
    }
}
