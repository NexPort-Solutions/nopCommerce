using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Plugin.Sale.PurchaseForCustomer.Models;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Factories
{
    public class PurchaseForCustomerModelFactory : IPurchaseForCustomerModelFactory
    {
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;

        public PurchaseForCustomerModelFactory(
            IStoreService storeService,
            ICustomerService customerService)
        {
            _storeService = storeService;
            _customerService = customerService;
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
