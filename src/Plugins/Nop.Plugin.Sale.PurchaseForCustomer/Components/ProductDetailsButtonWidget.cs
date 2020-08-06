using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Components
{
    [ViewComponent(Name = "ProductDetailsButtonWidget")]
    public class ProductDetailsButtonWidget : NopViewComponent
    {
        public ProductDetailsButtonWidget()
        {

        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = (ProductModel)additionalData;

            if (model == null)
                return Content("");

            return View("~/Plugins/Sale.PurchaseForCustomer/Areas/Admin/Views/Widget/ProductDetailsButtonWidget.cshtml", model);
        }
    }
}
