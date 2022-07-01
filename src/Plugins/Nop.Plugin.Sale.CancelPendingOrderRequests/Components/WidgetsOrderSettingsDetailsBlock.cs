using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Components
{
    [ViewComponent(Name = "WidgetsOrderSettingsDetailsBlock")]
    public class WidgetsOrderSettingsDetailsBlock : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var orderSettingsModel = (OrderSettingsModel)additionalData;

            if (orderSettingsModel == null)
                return Content("");

            var model  = new CancellationRequestSettingsModel();
            model.CancellationRequestReasonSearchModel.SetGridPageSize();

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Areas/Admin/Views/Widget/Setting/WidgetsOrderSettingsDetailsBlock.cshtml",
                model);
        }
    }
}
