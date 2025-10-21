using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Misc.Nexport.Filters;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

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

        //endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.MapProductPopup",
        //    "Admin/NexportIntegration/MapProductPopup",
        //    new { controller = "NexportIntegration", action = "MapProductPopup" });

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
        endpointRouteBuilder.MapControllerRoute("CategoryList",
            "admin/category/list",
            new { area = "Admin", controller = "NexportCategory", action = "List" });
        endpointRouteBuilder.MapControllerRoute("ReturnRequestList",
            "admin/returnrequest/list",
            new { area = "Admin", controller = "NexportReturnRequest", action = "List" });
        endpointRouteBuilder.MapControllerRoute("ReturnRequestEdit",
            "admin/returnrequest/nexport/edit/{id:min(0)}",
            new { area = "Admin", controller = "NexportReturnRequest", action = "Edit" });

        //endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Groups",
        //    "Admin/NexportIntegration/NexportGroups",
        //    new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroups" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Products",
            "Admin/Wholesale/NexportGroups/Products",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroupProducts" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Product.Redemptions",
            "Admin/Wholesale/NexportGroups/Products/Redemptions/",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroupProductRedemptions" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Product.Redemptions.WithParameters",
            "Admin/Wholesale/NexportGroups/Products/Redemptions/{productId:min(0)}/{groupId:guid?}",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportGroupProductRedemptions" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Group.Product.Redemptions.Redeem",
            "Admin/Wholesale/NexportGroups/Products/Redemptions/Redeem",
            new { area = "Admin", controller = "NexportWholesale", action = "RedeemProduct" });


        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Wholesale.Purchase.ByFundingPools.List",
            "Admin/Wholesale/Purchases/ByFundingPools/List",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportWholesalePurchasesByFundingPoolsList" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Wholesale.Purchase.ByFundingPools.Redemptions",
            "Admin/Wholesale/Purchases/ByFundingPools/Redemptions/",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportWholesalePurchasesByFundingPoolsRedemptionList" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Wholesale.Purchase.ByFundingPools.Redemptions.WithParameters",
            "Admin/Wholesale/Purchases/ByFundingPools/Redemptions/{fundingPoolId:int?}",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportWholesalePurchasesByFundingPoolsRedemptionList" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Wholesale.Purchase.ByFundingPools.Redemptions.Redeem",
            "Admin/Wholesale/Purchases/RedemptionAuditLog/List",
            new { area = "Admin", controller = "NexportWholesale", action = "RedemptionAuditLogList" });

        //endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Groups",
        //    "customer/nexportgroups",
        //    new { controller = "NexportWholesale", action = "CustomerNexportGroups" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Admin.Wholesale.Purchase.RedemptionAuditLog.List",
            "Admin/Wholesale/Purchases/ByFundingPools/List",
            new { area = "Admin", controller = "NexportWholesale", action = "AdminNexportWholesalePurchasesByFundingPoolsList" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Products",
            "customer/nexportgroups/products",
            new { controller = "NexportWholesale", action = "CustomerNexportGroupProducts" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions",
            "customer/nexportgroups/products/redemptions/",
            new { controller = "NexportWholesale", action = "CustomerNexportGroupProductRedemptions" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions.WithParameters",
            "customer/nexportgroups/products/redemptions/{productId:min(0)}/{groupId:guid?}",
            new { controller = "NexportWholesale", action = "CustomerNexportGroupProductRedemptions" });

        endpointRouteBuilder.MapControllerRoute("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions.Redeem",
            "customer/nexportgroups/products/redemptions/redeem",
            new { controller = "NexportWholesale", action = "RedeemProduct" });

        endpointRouteBuilder.MapControllerRoute(name: "RedeemByEmail",
            pattern: "redeembyemail/{invoiceItemId:min(0)}",
            defaults: new { controller = "NexportWholesale", action = "RedeemByEmail" });

        endpointRouteBuilder.MapControllerRoute(name: "VerifyExistingEnrollmentForRedemption",
            "verifyexistingenrollmentforredemption",
            new { controller = "NexportIntegration", action = "VerifyExistingEnrollmentForRedemption" });

        endpointRouteBuilder.MapControllerRoute(name: "ReturnRequest",
            pattern: "returnrequest/{orderId:min(0)}",
            new { controller = "NexportReturnRequest", action = "ReturnRequest" });

        endpointRouteBuilder.MapHangfireDashboard("/Admin/Hangfire", new DashboardOptions()
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }
}