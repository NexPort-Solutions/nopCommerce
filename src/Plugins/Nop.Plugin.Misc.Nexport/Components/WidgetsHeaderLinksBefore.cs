using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsHeaderLinksBefore")]
    public class WidgetsHeaderLinksBefore : NopViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly NexportService _nexportService;
        private readonly NexportSettings _nexportSettings;

        public WidgetsHeaderLinksBefore(
            ICustomerService customerService,
            IWorkContext workContext,
            NexportService nexportService,
            NexportSettings nexportSettings,
            IStoreContext storeContext)
        {
            _customerService = customerService;
            _workContext = workContext;
            _nexportService = nexportService;
            _nexportSettings = nexportSettings;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Content("");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            ViewData["CustomerName"] = $"{customer.FirstName} {customer.LastName}";

            var showNexportWholesalePurchases = await _nexportService.HasWholesaleOrderInfo(null, store, customer);

            //didnt find wholesale orders for customer so check if purchasing agent and if so see if there
            //are orders for any of the groups customer is purchasing agent on
            if (!showNexportWholesalePurchases)
            {
                var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                if (userMapping == null)
                {
                    showNexportWholesalePurchases = false;
                }
                else
                {
                    var searchGroupsForPermissionResult = await _nexportService.SearchGroupsForPermissionAsync(
                        userMapping.NexportUserId,
                        _nexportSettings.RootOrganizationId.Value);

                    foreach (var item in searchGroupsForPermissionResult)
                    {
                        if (await _nexportService.HasWholesaleOrderInfo(item.Id, store))
                        {
                            showNexportWholesalePurchases = true;
                            break;
                        }
                    }
                }
            }

            ViewData["ShowNexportWholesalePurchases"] = showNexportWholesalePurchases;

            return View("~/Plugins/Misc.Nexport/Views/Widget/WidgetsHeaderLinksBefore.cshtml");
        }
    }
}
