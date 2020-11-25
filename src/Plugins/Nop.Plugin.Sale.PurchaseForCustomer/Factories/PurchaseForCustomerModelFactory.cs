using System.Linq;
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

        public PurchaseForCustomerOrderModel PreparePurchaseForCustomerOrderModel(int productId)
        {
            var model = new PurchaseForCustomerOrderModel
            {
                ProductId = productId
            };

            var availableStores = _storeService.GetAllStores();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString()
            }).ToList();

            var availableCustomers = _customerService.GetAllCustomers().ToList()
                .Where(customer => !customer.IsSystemAccount &&
                            !string.IsNullOrWhiteSpace(customer.Email) && _customerService.IsRegistered(customer))
                .OrderBy(customer => customer.Email);
            model.AvailableCustomers = availableCustomers.Select(customer =>
            {
                var customerFullName = _customerService.GetCustomerFullName(customer);
                return new SelectListItem()
                {
                    Text = $"{customer.Email} - {customerFullName}",
                    Value = customer.Id.ToString()
                };
            }).ToList();

            return model;
        }
    }
}
