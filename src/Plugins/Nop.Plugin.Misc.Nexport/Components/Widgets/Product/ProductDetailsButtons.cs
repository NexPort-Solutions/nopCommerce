using Nop.Core;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Product;

public class ProductDetailsButtons : NopViewComponent
{
    private readonly IStoreContext _storeContext;

    public ProductDetailsButtons(IStoreContext storeContext) => _storeContext = storeContext;

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (await _storeContext.GetCurrentStoreAsync() is null
            || additionalData is not ProductModel productModel)
        {
            return Content(string.Empty);
        }
        return View(productModel);
    }
}
