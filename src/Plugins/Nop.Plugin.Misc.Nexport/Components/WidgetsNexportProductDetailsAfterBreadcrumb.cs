using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Widget;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsNexportProductDetailsAfterBreadcrumb")]
public class WidgetsNexportProductDetailsAfterBreadcrumb : NopViewComponent
{
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IOrderService _orderService;
    private readonly NexportService _nexportService;
    private readonly IDateTimeHelper _dateTimeHelper;

    public WidgetsNexportProductDetailsAfterBreadcrumb(
        NexportService nexportService,
        IOrderService orderService,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IDateTimeHelper dateTimeHelper,
        INexportPluginModelFactory nexportPluginModelFactory)
    {
        _nexportService = nexportService;
        _orderService = orderService;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _dateTimeHelper = dateTimeHelper;
        _nexportPluginModelFactory = nexportPluginModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var productDetailsModel = (ProductDetailsModel)additionalData;
        if (productDetailsModel == null)
            return Content("");

        var store = await _storeContext.GetCurrentStoreAsync();
        if (store == null)
            return Content("");

        var displayLastPurchaseInfo = await _genericAttributeService.GetAttributeAsync<bool>(store, NexportDefaults.DISPLAY_LAST_PURCHASE_INFO, store.Id);
        if (!displayLastPurchaseInfo)
            return Content("");

        var customer = await _workContext.GetCurrentCustomerAsync();

        var previousOrders = await _orderService.SearchOrdersAsync(store.Id, customerId: customer.Id, productId: productDetailsModel.Id);
        var totalPurchases = previousOrders.TotalCount;
        var lastPurchaseOrder = previousOrders.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();

        var model = new NexportProductDetailsAfterBreadcrumbWidgetModel
        {
            TotalPurchases = totalPurchases,
            LastPurchaseOrderId = lastPurchaseOrder?.Id,
            LastPurchaseDate = lastPurchaseOrder != null ? await _dateTimeHelper.ConvertToUserTimeAsync(lastPurchaseOrder.CreatedOnUtc, DateTimeKind.Utc) : null
        };

        return View("~/Plugins/Misc.Nexport/Views/Widget/Product/WidgetsNexportProductDetailsAfterBreadcrumb.cshtml", model);
    }
}