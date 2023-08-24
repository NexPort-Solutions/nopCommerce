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



            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Groups",
                "Admin/NexportIntegration/NexportGroups",
                new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroups" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Products",
                "Admin/NexportIntegration/NexportGroups/Products",
                new {  area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroupProducts" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Product.Redemptions",
                "Admin/NexportIntegration/NexportGroups/Products/Redemptions",
                new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroupProductRedemptions" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Product.Redemptions.RedeemOrModify",
                "Admin/NexportIntegration/NexportGroups/Products/Redemptions/RedeemOrModify",
                new {area = "Admin", controller = "NexportWholesale", action = "RedeemOrModify" });



            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Groups",
                "customer/nexportgroups",
                new { controller = "NexportWholesale", action = "CustomerNexportGroups" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Products",
                "customer/nexportgroups/products",
                new { controller = "NexportWholesale", action = "CustomerNexportGroupProducts" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions",
                "customer/nexportgroups/products/redemptions",
                new { controller = "NexportWholesale", action = "CustomerNexportGroupProductRedemptions" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions.RedeemOrModify",
                "customer/nexportgroups/products/redemptions/redeemormodify",
                new { controller = "NexportWholesale", action = "RedeemOrModify" });

            
        }
    }
}
 