using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Components;

[ViewComponent(Name = "WidgetsNexportProductDetailsAfterPictures")]
public class WidgetsNexportProductBoxAddInfoBefore : NopViewComponent
{
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _cacheManager;
    private readonly ISettingService _settingService;
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IWorkContext _workContext;
    private readonly IStoreModelFactory _storeModelFactory;
    private readonly IProductModelFactory _productModelFactory;
    private readonly NexportService _nexportService;

    public WidgetsNexportProductBoxAddInfoBefore(
        NexportService nexportService,
        IProductModelFactory productModelFactory,
        IStoreModelFactory storeModelFactory,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager,
        ISettingService settingService,
        INexportPluginModelFactory nexportPluginModelFactory,
        IWorkContext workContext)
    {
        _nexportService = nexportService;
        _productModelFactory = productModelFactory;
        _storeModelFactory = storeModelFactory;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
        _settingService = settingService;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        if (store == null)
            return Content("");

        var customer = await _workContext.GetCurrentCustomerAsync();

        var productOverviewModel = (ProductOverviewModel)additionalData;
        if (productOverviewModel == null)
            return Content("");

        var model = await _nexportPluginModelFactory.PrepareNexportProductRedemptionStatusesModel(customer, productOverviewModel.Id, store.Id);

        return await ViewAsync("~/Plugins/Misc.Nexport/Views/Widget/Product/NexportProductBoxAddInfoBefore.cshtml", model);
    }
}