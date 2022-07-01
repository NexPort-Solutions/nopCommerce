using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportStoreDetails")]
    public class WidgetsNexportStoreDetails : NopViewComponent
    {
        private readonly NexportSettings _nexportSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreModelFactory _storeModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _genericAttributeService;

        public WidgetsNexportStoreDetails(
            NexportSettings nexportSettings,
            IStaticCacheManager cacheManager,
            IStoreModelFactory storeModelFactory,
            ISettingService settingService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService)
        {
            _nexportSettings = nexportSettings;
            _cacheManager = cacheManager;
            _storeModelFactory = storeModelFactory;
            _settingService = settingService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
                return Content("");

            var storeModel = (StoreModel)additionalData;
            var store = await _storeService.GetStoreByIdAsync(storeModel.Id);

            if (store == null)
                return Content("");

            var model = store.ToModel<NexportStoreModel>();

            model.NexportSubscriptionOrgId = await _genericAttributeService.GetAttributeAsync<Guid?>(store,
                NexportDefaults.NEXPORT_SUBSCRIPTION_ORGANIZATION_ID_SETTING_KEY, store.Id);
            model.HideSectionCEUsInProductPage = await _genericAttributeService.GetAttributeAsync<bool>(store,
                NexportDefaults.HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY, store.Id);
            model.HideAddToCartForIneligibleProducts = await _genericAttributeService.GetAttributeAsync<bool>(store,
                NexportDefaults.HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id);
            model.SaleModel = await _genericAttributeService.GetAttributeAsync<NexportStoreSaleModel>(store,
                NexportDefaults.NEXPORT_STORE_SALE_MODEL_SETTING_KEY, store.Id);
            model.AllowRepurchaseFailedCourses = await _genericAttributeService.GetAttributeAsync<bool>(store,
                    NexportDefaults.ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
            model.AllowRepurchasePassedCourses = await _genericAttributeService.GetAttributeAsync<bool>(store,
                NexportDefaults.ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Store/NexportStoreDetails.cshtml", model);
        }
    }
}
