using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Components
{
    [ViewComponent(Name = "WidgetsOrderDetailsPageOverview")]
    public class WidgetsOrderDetailsPageOverview : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;

        public WidgetsOrderDetailsPageOverview(
            IOrderService orderService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _orderService = orderService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (_storeContext.CurrentStore == null)
                return Content("");

            var orderDetailsModel = (OrderDetailsModel)additionalData;

            if (orderDetailsModel == null)
                return Content("");

            var order = _orderService.GetOrderById(orderDetailsModel.Id);
            if (order == null || order.OrderStatus != OrderStatus.Pending)
                return Content("");

            return View("~/Plugins/Sale.CancelPendingOrderRequests/Views/Widget/Order/WidgetsOrderDetailsPageOverview.cshtml", orderDetailsModel);
        }
    }
}
