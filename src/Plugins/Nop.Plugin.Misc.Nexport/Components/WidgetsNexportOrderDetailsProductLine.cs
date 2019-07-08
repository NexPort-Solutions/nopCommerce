using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Order;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportOrderDetailsProductLine")]
    public class WidgetsNexportOrderDetailsProductLine : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IStoreModelFactory _storeModelFactory;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IOrderService _orderService;
        private readonly NexportService _nexportService;

        public WidgetsNexportOrderDetailsProductLine(
            NexportService nexportService,
            IProductModelFactory productModelFactory,
            IStoreModelFactory storeModelFactory,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ISettingService settingService,
            IOrderService orderService)
        {
            _nexportService = nexportService;
            _productModelFactory = productModelFactory;
            _storeModelFactory = storeModelFactory;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _settingService = settingService;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = (OrderDetailsModel.OrderItemModel)additionalData;

            var orderItem = _orderService.GetOrderItemByGuid(model.OrderItemGuid);

            if (orderItem.Order.OrderStatus != OrderStatus.Complete)
                return Content("");

            return View("~/Plugins/Misc.Nexport/Views/Widget/Order/NexportOrderDetailsProductLine.cshtml", model);
        }
    }
}
