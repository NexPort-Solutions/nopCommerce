using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;
using Nop.Plugin.Sale.PurchaseForCustomer.Models.Plugins;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories
{
    public class PurchaseForCustomerModelFactory : IPurchaseForCustomerModelFactory
    {
        private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;

        public PurchaseForCustomerModelFactory(
            IRepository<LocaleStringResource> localeStringResourceRepository,
            IStoreService storeService,
            ICustomerService customerService)
        {
            _localeStringResourceRepository = localeStringResourceRepository;
            _storeService = storeService;
            _customerService = customerService;
        }

        public Task<PurchaseForCustomerPluginResourceListModel> PreparePurchaseForCustomerPluginResourceListModelAsync(
            PurchaseForCustomerPluginResourceListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var results = new List<LocaleStringResource>();

            foreach (var localeResource in PurchaseForCustomerPluginService.GetLocaleResource())
            {
                var lsrList = _localeStringResourceRepository.Table
                    .Where(l => l.ResourceName == localeResource.Key && l.ResourceValue != localeResource.Value)
                    .ToList();

                results.AddRange(lsrList);
            }

            var resources = new PagedList<LocaleStringResource>(results, searchModel.Page - 1, searchModel.PageSize);

            // prepare to grid
            var model = new PurchaseForCustomerPluginResourceListModel().PrepareToGrid(searchModel, resources, () =>
            {
                return resources.Select(resource =>
                {
                    var localeResourceModel = resource.ToModel<LocaleResourceModel>();
                    return localeResourceModel;
                });
            });

            // the interface allows asynchronous callers, but this method is synchronous
            return Task.FromResult(model);
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


            var availableCustomers = (await _customerService.GetAllCustomersAsync()).ToList()
                .WhereAwait(async customer => !customer.IsSystemAccount &&
                            !string.IsNullOrWhiteSpace(customer.Email) && await _customerService.IsRegisteredAsync(customer))
                .OrderBy(customer => customer.Email);
            model.AvailableCustomers = await availableCustomers.SelectAwait(async customer =>
            {
                var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                return new SelectListItem()
                {
                    Text = $"{customer.Email} - {customerFullName}",
                    Value = customer.Id.ToString()
                };
            }).ToListAsync();

            return model;
        }
    }
}
