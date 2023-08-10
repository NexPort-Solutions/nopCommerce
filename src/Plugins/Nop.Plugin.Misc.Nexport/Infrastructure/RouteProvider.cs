using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue - 100;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Configure",
                "Admin/NexportIntegration/Configure",
                new { controller = "NexportIntegration", action = "Configure" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Configure.SetRootOrganization",
                "Admin/NexportIntegration/SetRootOrganization",
                new { controller = "NexportIntegration", action = "SetRootOrganization" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.MapProductPopup",
                "Admin/NexportIntegration/MapProductPopup",
                new { controller = "NexportIntegration", action = "MapProductPopup" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.MyTraining",
                "customer/nexporttraining",
                new { controller = "NexportIntegration", action = "ViewNexportTraining" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers",
                "customer/nexportsuplementalinfoanswers",
                new { controller = "NexportIntegration", action = "ViewSupplementalInfoAnswers" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.SupplementalInfoAnswers.CustomerEdit",
                "customer/nexportsuplementalinfoanswers/edit/{questionId:min(0)}",
                new { controller = "NexportIntegration", action = "EditSupplementalInfoAnswers" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.RedeemOrder",
                "customer/redeem",
                new { controller = "NexportIntegration", action = "RedeemNexportOrderInvoiceItem" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.GoToNexport",
                "customer/transfertonexport",
                new { controller = "NexportIntegration", action = "GoToNexport" });

            endpointRouteBuilder.MapControllerRoute("NexportLogin",
                "login/",
                new { controller = "NexportCustomer", action = "Login", });
            endpointRouteBuilder.MapControllerRoute("NexportLoginCheckoutAsGuest",
                "login/checkoutasguest",
                new { controller = "NexportCustomer", action = "Login", checkoutAsGuest = true });
            endpointRouteBuilder.MapControllerRoute("NexportRegistration",
                "register/",
                new { controller = "NexportCustomer", action = "Register" });
            endpointRouteBuilder.MapControllerRoute("StoreList",
                "admin/store/list",
                new { area = "Admin", controller = "NexportStore", action = "List" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Groups",
                "customer/nexportgroups",
                new { controller = "NexportWholesale", action = "CustomerNexportGroups" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Groups.Products",
                "customer/nexportgroups/products",
                new { controller = "NexportWholesale", action = "CustomerNexportGroupProducts" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Groups.Products.Redemptions",
                "customer/nexportgroups/products/redemptions",
                new { controller = "NexportWholesale", action = "CustomerNexportGroupProductRedemptions" });

            
        }
    }
}
 