using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();

            routeBuilder.MapRoute("Plugin.Misc.Nexport.Archway.Configure",
                "Admin/ArchwayEmployeeRegistrationField/Configure",
                new { controller = "ArchwayEmployeeRegistrationField", action = "Configure" });

            routeBuilder.MapRoute("Plugin.Misc.Nexport.Archway.CustomRender",
                "Admin/ArchwayEmployeeRegistrationField/CustomRender",
                new { controller = "ArchwayEmployeeRegistrationField", action = "CustomRender" });

            routeBuilder.MapRoute("GetArchwayStoreCitiesByState", "ArchwayEmployeeRegistrationField/getarchwaystorecitiesbystate/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreCitiesByState" });

            routeBuilder.MapRoute("GetArchwayStoreAddressesByCity", "ArchwayEmployeeRegistrationField/getarchwaystoreaddressesbycity/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreAddressesByCity" });

            routeBuilder.MapRoute("GetArchwayEmployeePositionsByStore", "ArchwayEmployeeRegistrationField/getarchwayemployeepositionsbystore/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreEmployeePositionsByStore" });

            if (settingService != null && storeContext != null)
            {
                var customEnrollmentRouteSetting = settingService.GetSetting("nexport.archway.enrollment.route",
                    storeContext.CurrentStore.Id, true);
                if (customEnrollmentRouteSetting != null)
                {
                    var customEnrollmentRoute = !string.IsNullOrWhiteSpace(customEnrollmentRouteSetting.Value)
                        ? customEnrollmentRouteSetting.Value
                        : "enroll/";

                    routeBuilder.MapLocalizedRoute("ArchwayCustomEnrollmentRoute", customEnrollmentRoute,
                        new {controller = "ShoppingCart", action = "Cart"});
                }
            }
        }

        public int Priority => int.MaxValue - 99;
    }
}
