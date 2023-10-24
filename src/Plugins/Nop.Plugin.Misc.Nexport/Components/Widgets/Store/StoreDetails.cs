using Nop.Services.Common;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Components;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Store;

public class StoreDetails : NopViewComponent
{
    private readonly Settings _settings;
    private readonly IStoreService _store;
    private readonly IGenericAttributeService _generics;

    public StoreDetails(
        Settings settings,
        IStoreService storeService,
        IGenericAttributeService genericAttributeService)
    {
        _settings = settings;
        _store = storeService;
        _generics = genericAttributeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken)
            || additionalData is not Web.Areas.Admin.Models.Stores.StoreModel { Id: var id }
            || await _store.GetStoreByIdAsync(id) is not { } store)
        {
            return Content(string.Empty);
        }

        var model = store.ToModel<Models.Stores.StoreModel>();
        model.SubscriptionOrgId = await _generics.GetAttributeAsync<Guid?>(store, SUBSCRIPTION_ORGANIZATION_ID_SETTING_KEY, store.Id);
        model.HideSectionCEUsInProductPage = await _generics.GetAttributeAsync<bool>(store, HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY, store.Id);
        model.HideAddToCartForIneligibleProducts = await _generics.GetAttributeAsync<bool>(store, HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, store.Id);
        model.AllowRepurchaseFailedCourses = await _generics.GetAttributeAsync<bool>(store, ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
        model.AllowRepurchasePassedCourses = await _generics.GetAttributeAsync<bool>(store, ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, store.Id);
        return View(model);
    }
}
