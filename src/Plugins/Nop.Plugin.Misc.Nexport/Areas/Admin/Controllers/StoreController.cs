using System.Globalization;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
[Route(template: "[area]/[controller]/[action]", Order = int.MinValue)]
public class StoreController : Web.Areas.Admin.Controllers.StoreController
{
    private readonly IPermissionService _permission;
    private readonly Factories.IPluginModelFactory _model;
    private readonly IStoreService _store;
    private readonly INotificationService _notification;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IProductMappingService _productMapping;

    public StoreController(
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreModelFactory storeModelFactory,
        IStoreService store,
        IGenericAttributeService genericAttributeService,
        IWebHelper webHelper,
        IWorkContext workContext,
        Factories.IPluginModelFactory pluginModelFactory,
        IGenericAttributeService genericAttribute,
        INotificationService notification,
        IProductMappingService productMapping)
            : base(
                customerActivityService,
                localizationService,
                localizedEntityService,
                notificationService,
                permissionService,
                settingService,
                storeModelFactory,
                store,
                genericAttributeService,
                webHelper,
                workContext)
    {
        _permission = permissionService;
        _model = pluginModelFactory;
        _store = store;
        _genericAttribute = genericAttribute;
        _notification = notification;
        _productMapping = productMapping;
    }

    [HttpGet]
    public override async Task<IActionResult> List()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        {
            return AccessDeniedView();
        }

        var model = new StoreSearchModel { SearchStoreName = string.Empty, SearchStoreUrl = string.Empty };
        return base.View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(StoreSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.StoreListModel(model));
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchStores(string term)
    {
        var stores = (await _store.GetAllStoresAsync())
            .Where(store => store.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(store => new JQueryObject<string>(store.Name, store.Id.ToString(CultureInfo.InvariantCulture)));
        return Json(stores);
    }

    [Route("Admin/Store/Edit/{newMapping}")]
    [HttpPost]
    [ActionName("Edit")]
    [FormValueRequired("setsubscriptionorgid")]
    public async Task<IActionResult> SetSubscriptionOrganizationId(Web.Areas.Admin.Models.Stores.StoreModel model, [FromForm(Name = "SubscriptionOrgId")] Guid subOrgId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        {
            return AccessDeniedView();
        }

        var store = await _store.GetStoreByIdAsync(model.Id);
        if (store is null)
        {
            return RedirectToAction("List", "Store");
        }

        await _genericAttribute.SaveAttributeAsync(store, "SubscriptionOrganizationId", subOrgId, store.Id);
        _notification.SuccessNotification("Success update NexPort subscription organization");
        return RedirectToAction("Edit", "Store", new { id = store.Id });
    }

    [Route("Admin/Store/Edit/{newMapping}")]
    [HttpPost]
    [ActionName("Edit")]
    [FormValueRequired("savenexportstoreconfig")]
    public async Task<IActionResult> SaveStoreConfiguration(StoreModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        {
            return AccessDeniedView();
        }

        var store = await _store.GetStoreByIdAsync(model.Id);
        if (store is null)
        {
            return RedirectToAction("List", "Store");
        }

        await _genericAttribute.SaveAttributeAsync(store, ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, model.AllowRepurchaseFailedCourses, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, model.AllowRepurchasePassedCourses, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY, model.HideSectionCEUsInProductPage, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, HIDE_ADD_TO_CART_FOR_INELIGIBLE_PRODUCTS_SETTING_KEY, model.HideAddToCartForIneligibleProducts, store.Id);
        _notification.SuccessNotification("Success update NexPort store configuration");
        return RedirectToAction("Edit", "Store", new { id = store.Id });
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityInsertedEvent<Store> eventMessage)
    {
        var store = eventMessage.Entity;
        await _genericAttribute.SaveAttributeAsync(store, STORE_SALE_MODEL_SETTING_KEY, StoreSaleModel.Retail, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, ALLOW_REPURCHASE_FAILED_COURSES_FROM_NEXPORT_SETTING_KEY, true, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, ALLOW_REPURCHASE_PASSED_COURSES_FROM_NEXPORT_SETTING_KEY, false, store.Id);
        await _genericAttribute.SaveAttributeAsync(store, HIDE_SECTION_CEUS_IN_PRODUCT_PAGE_SETTING_KEY, false, store.Id);
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityDeletedEvent<Store> eventMessage)
    {
        var deletedStore = eventMessage.Entity;
        // Find and remove all generic attributes that associated with this store
        var storeAttributes = await _genericAttribute.GetAttributesForEntityAsync(deletedStore.Id, "Store");
        await _genericAttribute.DeleteAttributesAsync(storeAttributes);
        // Find and remove product mappings that are associated with this store
        var mappings = await _productMapping.GetAllByStoreId(deletedStore.Id);
        foreach (var mapping in mappings)
        {
            await _productMapping.Delete(mapping);
        }
    }
}
