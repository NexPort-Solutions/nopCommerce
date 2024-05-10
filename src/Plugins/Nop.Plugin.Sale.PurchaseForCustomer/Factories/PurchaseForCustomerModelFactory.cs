using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Services.Stores;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories;

public class PurchaseForCustomerModelFactory : IPurchaseForCustomerModelFactory
{
    private readonly PurchaseForCustomerPluginService _pluginService;
    private readonly IStoreService _storeService;

    public PurchaseForCustomerModelFactory(
        PurchaseForCustomerPluginService pluginService,
        IStoreService storeService)
    {
        _pluginService = pluginService;
        _storeService = storeService;
    }

    public async Task<PurchaseForCustomerPluginResourceListModel> PreparePurchaseForCustomerPluginResourceListModelAsync(
        PurchaseForCustomerPluginResourceListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var results = await _pluginService.GetConflictedLocalizedResourcesAsync();

        var resources = new PagedList<LocaleStringResource>(results, searchModel.Page - 1, searchModel.PageSize);

        var model = await new PurchaseForCustomerPluginResourceListModel().PrepareToGridAsync(searchModel, resources, () =>
        {
            return resources.SelectAwait(async resource =>
            {
                var localeResourceModel = new LocaleResourceModel
                {
                    Id = resource.Id,
                    ResourceValue = resource.ResourceValue,
                    ResourceName = resource.ResourceName,
                    LanguageId = resource.LanguageId
                };

                return localeResourceModel;
            });
        });

        return model;
    }

    public async Task<PurchaseForCustomerOrderModel> PreparePurchaseForCustomerOrderModel(int productId)
    {
        var model = new PurchaseForCustomerOrderModel
        {
            ProductId = productId
        };

        var availableStores = await _storeService.GetAllStoresAsync();
        model.AvailableStores = availableStores.Select(store => new SelectListItem
        {
            Text = store.Name,
            Value = store.Id.ToString()
        }).ToList();

        return model;
    }
}