using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Archway;
using Nop.Services.Configuration;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public async void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var storeContext = EngineContext.Current.Resolve<IStoreContext>();
        if (await settingService.GetSettingAsync(PluginDefaults.CUSTOM_ENROLLMENT_ROUTE_SETTING_KEY, storeContext.GetCurrentStore().Id, true) is { Value: var customEnrollmentRoute })
        {
            endpointRouteBuilder.MapControllerRoute("CustomEnrollmentRoute", customEnrollmentRoute ?? "enroll", new { controller = "ShoppingCart", action = "Cart" });
        }
    }

    public int Priority => int.MaxValue - 99;
}
