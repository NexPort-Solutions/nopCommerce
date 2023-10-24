using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Controller = Nop.Plugin.Misc.Nexport.Controllers;
using static Nop.Plugin.Misc.Nexport.Extensions.ViewUtilities;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public int Priority => int.MaxValue - 100;

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        const string admin = "Admin";

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Configure",
            "Admin/Integration/Configure",
            new { controller = GetControllerName<IntegrationController>(), action = nameof(IntegrationController.Configure) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Configure.SetRootOrganization",
            "Admin/Integration/SetRootOrganization",
            new { controller = GetControllerName<IntegrationController>(), action = nameof(IntegrationController.SetRootOrganization) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.MapProductPopup",
            "Admin/ProductMapping/MapProductPopup",
            new { controller = GetControllerName<ProductMappingController>(), action = nameof(ProductMappingController.MapProductPopup) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.MyTraining",
            "Customer/Training",
            new { controller = GetControllerName<IntegrationController>(), action = nameof(IntegrationController.ViewTraining) });

        endpointRouteBuilder.MapControllerRoute(
            nameof(SupplementalInfoController.ViewSupplementalInfoAnswers),
            "Customer/SuplementalInfoAnswers",
            new { controller = GetControllerName<SupplementalInfoController>(), action = nameof(SupplementalInfoController.ViewSupplementalInfoAnswers) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.SupplementalInfoAnswers.CustomerEdit",
            "Customer/suplementalinfoanswers/Edit/{questionId:min(0)}",
            new { controller = GetControllerName<SupplementalInfoController>(), action = nameof(SupplementalInfoController.EditSupplementalInfoAnswers) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.RedeemOrder",
            "Customer/Redeem",
            new { controller = GetControllerName<IntegrationController>(), action = nameof(IntegrationController.RedeemOrderInvoiceItem) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.GoToNexport",
            "Customer/TransferToNexport",
            new { controller = GetControllerName<IntegrationController>(), action = nameof(IntegrationController.GoToNexport) });

        endpointRouteBuilder.MapControllerRoute(
            nameof(Controller.CustomerController.Login),
            "Login",
            new { controller = GetControllerName<Controller.CustomerController>(), action = nameof(Controller.CustomerController.Login) });

        endpointRouteBuilder.MapControllerRoute(
            "LoginCheckoutAsGuest",
            "Login/Checkoutasguest",
            new { controller = GetControllerName<Controller.CustomerController>(), action = nameof(Controller.CustomerController.Login), checkoutAsGuest = true });

        endpointRouteBuilder.MapControllerRoute(
            "Registration",
            "Register",
            new { controller = GetControllerName<Controller.CustomerController>(), action = nameof(Controller.CustomerController.Register) });

        endpointRouteBuilder.MapControllerRoute(
            "StoreList",
            "Admin/Store/List",
            new { area = admin, controller = GetControllerName<StoreController>(), action = nameof(StoreController.List) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Admin.Groups",
            "Admin/Wholesale/Groups",
            new { area = admin, controller = GetControllerName<WholesaleController>(), action = nameof(WholesaleController.AdminGroups) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Admin.Group.Products",
            "Admin/Wholesale/Groups/Products",
            new { area = admin, controller = GetControllerName<WholesaleController>(), action = nameof(WholesaleController.GroupProductsAsync) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Admin.Group.Product.Redemptions",
            "Admin/Wholesale/Groups/Products/Redemptions",
            new { area = admin, controller = GetControllerName<WholesaleController>(), action = nameof(WholesaleController.GroupProductRedemptions) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Admin.Group.Product.Redemptions.RedeemOrModify",
            "Admin/Wholesale/Groups/Products/Redemptions/RedeemOrModify",
            new { area = admin, controller = GetControllerName<WholesaleController>(), action = nameof(WholesaleController.RedeemProduct) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Customer.Groups",
            "Customer/Groups",
            new { controller = GetControllerName<Controller.WholesaleController>(), action = nameof(Controller.WholesaleController.CustomerGroups) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Customer.Group.Products",
            "Customer/Groups/Products",
            new { controller = GetControllerName<Controller.WholesaleController>(), action = nameof(Controller.WholesaleController.CustomerGroupProducts) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Customer.Group.Product.Redemptions",
            "Customer/Groups/Products/Redemptions",
            new { controller = GetControllerName<Controller.WholesaleController>(), action = nameof(Controller.WholesaleController.GroupProductRedemptions) });

        endpointRouteBuilder.MapControllerRoute(
            "NexportPlugin.Misc.Nexport.Customer.Group.Product.Redemptions.RedeemOrModify",
            "Customer/Groups/Products/Redemptions/RedeemOrModify",
            new { controller = GetControllerName<Controller.WholesaleController>(), action = nameof(Controller.WholesaleController.RedeemProduct) });

        endpointRouteBuilder.MapControllerRoute(
            "Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers.OrderController.List",
            "Admin/Order/List",
            new { controller = GetControllerName<OrderController>(), action = nameof(OrderController.List) });
    }
}
