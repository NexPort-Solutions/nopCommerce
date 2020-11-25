using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Archway.Configure",
                "Admin/ArchwayEmployeeRegistrationField/Configure",
                new { controller = "ArchwayEmployeeRegistrationField", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Archway.CustomRender",
                "Admin/ArchwayEmployeeRegistrationField/CustomRender",
                new { controller = "ArchwayEmployeeRegistrationField", action = "CustomRender" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Archway.UploadStoreData",
                "Admin/ArchwayEmployeeRegistrationField/AsyncUploadStoreData",
                new { controller = "ArchwayEmployeeRegistrationField", action = "AsyncUploadStoreData" });

            endpointRouteBuilder.MapControllerRoute("GetArchwayStoreCitiesByState", "ArchwayEmployeeRegistrationField/getarchwaystorecitiesbystate/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreCitiesByState" });

            endpointRouteBuilder.MapControllerRoute("GetArchwayStoreAddressesByCity", "ArchwayEmployeeRegistrationField/getarchwaystoreaddressesbycity/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreAddressesByCity" });

            endpointRouteBuilder.MapControllerRoute("GetArchwayEmployeePositionsByStore", "ArchwayEmployeeRegistrationField/getarchwayemployeepositionsbystore/",
                new { controller = "ArchwayEmployeeRegistrationField", action = "GetArchwayStoreEmployeePositionsByStore" });

            if (settingService != null && storeContext != null)
            {
                var customEnrollmentRouteSetting = settingService.GetSetting(PluginDefaults.CustomEnrollmentRouteSettingKey,
                    storeContext.CurrentStore.Id, true);
                if (customEnrollmentRouteSetting != null)
                {
                    var customEnrollmentRoute = !string.IsNullOrWhiteSpace(customEnrollmentRouteSetting.Value)
                        ? customEnrollmentRouteSetting.Value
                        : "enroll";

                    endpointRouteBuilder.MapControllerRoute("ArchwayCustomEnrollmentRoute", customEnrollmentRoute,
                        new {controller = "ShoppingCart", action = "Cart"});
                }
            }
        }

        public int Priority => int.MaxValue - 99;
    }
}
