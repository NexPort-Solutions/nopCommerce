using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Stores;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories
{
    public class PurchaseForCustomerModelFactory : IPurchaseForCustomerModelFactory
    {
        private readonly IStoreService _storeService;

        public PurchaseForCustomerModelFactory(
            IStoreService storeService)
        {
            _storeService = storeService;
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
}
