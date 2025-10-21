using Hangfire.Dashboard;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.Nexport.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var result = false;
        Task.Run(async () =>
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var customer = await workContext.GetCurrentCustomerAsync();
            if (customer != null)
                result = await customerService.IsAdminAsync(customer);
        }).GetAwaiter().GetResult();

        return result;
    }
}