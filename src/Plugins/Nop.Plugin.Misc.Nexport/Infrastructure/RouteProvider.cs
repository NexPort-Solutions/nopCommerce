﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue - 100;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.Misc.Nexport.Configure",
                "Admin/NexportIntegration/Configure",
                new { controller = "NexportIntegration", action = "Configure" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.Configure.SetRootOrganization",
                "Admin/NexportIntegration/SetRootOrganization",
                new { controller = "NexportIntegration", action = "SetRootOrganization" });

            routeBuilder.MapRoute("Plugin.Misc.Nexport.MapProductPopup",
                "Admin/NexportIntegration/MapProductPopup",
                new { controller = "NexportIntegration", action = "MapProductPopup"});

            //routeBuilder.MapRoute("Plugin.Misc.Nexport.Customer.MapNexportUser",
            //    "customer/nexportusermapping",
            //    new { controller = "NexportIntegration", action = "MapNexportUser" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.MyTraining",
                "customer/nexporttraining",
                new { controller = "NexportIntegration", action = "ViewNexportTraining" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers",
                "customer/nexportsuplementalinfoanswers",
                new { controller = "NexportIntegration", action = "ViewSupplementalInfoAnswers" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers.CustomerEdit",
                "customer/nexportsuplementalinfoanswers/edit/{questionId:min(0)}",
                new { controller = "NexportIntegration", action = "EditSupplementalInfoAnswers" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.RedeemOrder",
                "customer/redeem",
                new { controller = "NexportIntegration", action = "RedeemNexportOrderInvoiceItem" });
            routeBuilder.MapRoute("Plugin.Misc.Nexport.GoToNexport",
                "customer/transfertonexport",
                new { controller = "NexportIntegration", action = "GoToNexport" });

            routeBuilder.MapLocalizedRoute("NexportLogin",
                "login/",
                new { controller = "NexportCustomer", action = "Login" });

            routeBuilder.MapLocalizedRoute("NexportLoginCheckoutAsGuest",
                "login/checkoutasguest",
                new { controller = "NexportCustomer", action = "Login", checkoutAsGuest = true });
        }
    }
}
