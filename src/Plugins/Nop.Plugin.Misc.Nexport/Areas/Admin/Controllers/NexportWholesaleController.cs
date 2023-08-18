using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Wholesale.PurchasingAgent;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportWholesaleController : BaseAdminController
{
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly IStoreService _storeService;
    private readonly INexportWholesaleService _nexportWholesaleService;
    private readonly IWorkContext _workContext;
    private readonly NexportService _nexportService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _productService;
    private readonly IPluginService _pluginService;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IProductModelFactory _productModelFactory;

    public NexportWholesaleController(INexportPluginModelFactory nexportPluginModelFactory, IPermissionService permissionService, IOrderService orderService, IStoreService storeService, INexportWholesaleService nexportWholesaleService, IWorkContext workContext, NexportService nexportService, IPaymentPluginManager paymentPluginManager, IProductService productService, IPluginService pluginService, ICustomerModelFactory customerModelFactory,
        IProductModelFactory productModelFactory)
    {
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _permissionService = permissionService;
        _storeService = storeService;
        _nexportWholesaleService = nexportWholesaleService;
        _workContext = workContext;
        _nexportService = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _productService = productService;
        _pluginService = pluginService;
        _customerModelFactory = customerModelFactory;
        _productModelFactory = productModelFactory;
    }

    [Route("Admin/Wholesale/Create")]
    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return AccessDeniedView();
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active || await _nexportService.FindUserMappingByCustomerId(customer.Id) is null)
        {
            return AccessDeniedView();
        }
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync();
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleCreateModel model)
    {
        var validation = await ValidateWholesaleOrderAsync(model);
        return validation switch
        {
            WholesaleOrderValidationResult.AccessDeniedResult => AccessDeniedView(),
            WholesaleOrderValidationResult.BadResult bad => await placeWholesaleOrder(bad),
            WholesaleOrderValidationResult.GoodResult result => await HandleGoodWholesaleOrderAsync(result),
            _ => throw new NotImplementedException()
        };
        async Task<ViewResult> placeWholesaleOrder(WholesaleOrderValidationResult.BadResult bad)
        {
            var newModel = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync();
            newModel.Error = bad.Error;
            return View("Create", newModel);
        }
    }

    private async Task<IActionResult> HandleGoodWholesaleOrderAsync(WholesaleOrderValidationResult.GoodResult result)
    {
        var shoppingCartItem = new ShoppingCartItem
        {
            ShoppingCartType = ShoppingCartType.ShoppingCart,
            StoreId = result.Store.Id,
            ProductId = result.Product.Id,
            AttributesXml = null,
            CustomerEnteredPrice = decimal.Zero,
            Quantity = result.Quantity,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
            CustomerId = result.Customer.Id,
        };
        var shoppingCartItems = new List<ShoppingCartItem> { shoppingCartItem };
        var processingPaymentRequest = new ProcessPaymentRequest
        {
            OrderGuid = Guid.NewGuid(),
            OrderGuidGeneratedOnUtc = DateTime.UtcNow,
            StoreId = result.Store.Id,
            CustomerId = result.Customer.Id,
            PaymentMethodSystemName = result.PaymentMethod
        };
        var placedOrderResult = await _nexportWholesaleService.PlaceWholesaleOrderAsync(processingPaymentRequest, shoppingCartItems);
        if (!placedOrderResult.Success)
        {
            throw new InvalidOperationException("Failed to place order");
        }
        return View("Redirect", new NextportWholesaleOrderCreationResponse { Id = placedOrderResult.PlacedOrder.Id });
    }

    private async Task<WholesaleOrderValidationResult> ValidateWholesaleOrderAsync(WholesaleCreateModel model)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var currentCustomerIsNotActive = customer is not { Active: true };
        var currentCustomerIsNotInNexport = await _nexportService.FindUserMappingByCustomerId(customer.Id) is null;
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
            || currentCustomerIsNotActive
            || currentCustomerIsNotInNexport)
        {
            ModelState.AddModelError("", "Current customer does not have permission for this resource.");
            return WholesaleOrderValidationResult.AccessDenied;
        }
        if (await _storeService.GetStoreByIdAsync(model.StoreId) is not { } store)
        {
            ModelState.AddModelError(nameof(model.StoreId), "Invalid store.");
        }
        else if (await GetPaymentMethodNameForStoreAndCustomerAsync(model.PaymentMethod, store, customer) is not { } paymentMethod)
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), $"{nameof(model.PaymentMethod)} {model.PaymentMethod} is not valid for {nameof(model.StoreId)} {store.Name}.");
        }
        else if (await _nexportService.GetOrganizationDetailsAsync(model.OrganizationId) is null)
        {
            ModelState.AddModelError(nameof(model.OrganizationId), "Invalid organization.");
        }
        else if (!model.IsRedemptionPeriodUnlimited && (model.RedeemByUtc is null || model.RedeemByUtc.Value <= DateTime.UtcNow))
        {
            ModelState.AddModelError(nameof(model.RedeemByUtc), $"{nameof(model.RedeemByUtc)} must be a date in the future or {nameof(model.IsRedemptionPeriodUnlimited)} must be true.");
        }
        else if (model.Quantity is > 100_000 or < 1)
        {
            ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
        }
        else if (await _productService.GetProductByIdAsync(model.ProductId) is not { } product)
        {
            ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
        }
        else if (ModelState.IsValid)
        {
            return WholesaleOrderValidationResult.Good(customer, store, paymentMethod, model.Quantity, product);
        }
        return WholesaleOrderValidationResult.Bad("Order parameters have invalid value: " + ModelState.ErrorCount + " errors.");
    }

    private async Task<string?> GetPaymentMethodNameForStoreAndCustomerAsync(string paymentMethodSystemName, Store store, Customer customer)
    {
        var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id);
        var paymentPluginName = activePlugins
            .Select(plugin => plugin.ToPluginModel<PaymentMethodModel>().SystemName)
            .FirstOrDefault(systemName => systemName == paymentMethodSystemName);
        return paymentPluginName;
    }

    private abstract record WholesaleOrderValidationResult
    {
        public static GoodResult Good(Customer customer, Store store, string paymentMethod, int quantity, Product product)
            => new(customer, store, paymentMethod, quantity, product);
        public static readonly AccessDeniedResult AccessDenied = new();
        public static BadResult Bad(string? error) => new(error);

        public record GoodResult(Customer Customer, Store Store, string PaymentMethod, int Quantity, Product Product) : WholesaleOrderValidationResult;
        public record AccessDeniedResult : WholesaleOrderValidationResult;

        public record BadResult(string? Error) : WholesaleOrderValidationResult;
    };

    public virtual async Task<IActionResult> AdminNexportGroups()
    {
        //TODO @js - permission for nexport groups list here
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        //    return AccessDeniedView();

        var searchModel = new NexportGroupListSearchModel();
        searchModel.AdminView = true;
       // searchModel.GroupProductsRoute = "Plugin.Misc.Nexport.Admin.Groups.Products";
        ViewData["NexportGroupsPath"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
        ViewData["NexportGroupsSearchModel"] = searchModel;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProducts(Guid groupId)
    {
        //var customer = await _workContext.GetCurrentCustomerAsync();
        //if (!await _customerService.IsRegisteredAsync(customer))
        //    return Challenge();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);
        searchModel.AdminView = true;
        ViewData["NexportGroupsPath"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
        ViewData["NexportGroupsSearchModel"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProductRedemptions(Guid groupId, int productId)
    {
        //var customer = await _workContext.GetCurrentCustomerAsync();
        //if (!await _customerService.IsRegisteredAsync(customer))
        //    return Challenge();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

        searchModel.AdminView = true;
        ViewData["NexportGroupsPath"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
        ViewData["NexportGroupsSearchModel"] = searchModel;
            
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
    {

        var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid groupId)
    {

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId);

        return Json(model);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductCustomerListModelAsync(searchModel, groupId, productId);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCount(groupId,productId);
        return Json(
            new {result = count}
        );
    }

    public virtual async Task<IActionResult> PurchasingAgentList()
    {
        //permission for purchasing agent list here
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        //    return AccessDeniedView();

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/PurchasingAgent/List.cshtml", new NexportPurchasingAgentSearchModel());
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public virtual async Task<IActionResult> PurchasingAgentList(NexportPurchasingAgentSearchModel searchModel)
    {
        //need permission for purchasing agent management
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageStores))
        //    return await AccessDeniedDataTablesJson();

        var smodel = await _customerModelFactory.PrepareCustomerSearchModelAsync(new CustomerSearchModel
        {
            AvailableCustomerRoles = new List<SelectListItem>(),
            AvailablePageSizes = null,
            CompanyEnabled = false,
            DateOfBirthEnabled = false,
            Draw = "1",
            FirstNameEnabled = false,
            LastNameEnabled = false,
            SearchDayOfBirth = "0",
            SearchMonthOfBirth = "0"
        });

        //prepare model
        var model = await _customerModelFactory.PrepareCustomerListModelAsync(smodel);

        return Json(model);
    }


    public async Task<IActionResult> AddPurchasingAgentGroup()
    {
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //    return AccessDeniedView();

        //var product = await _productService.GetProductByIdAsync(productId)
        //              ?? throw new Exception($"No product found with the specified id {productId}");

        //var model = await _nexportPluginModelFactory.PrepareDuplicateNexportProductMappingModel(product);

        return View($"{NexportDefaults.NexportPluginAdminViewBasePath}NexportWholesale/PurchasingAgent/Create.cshtml");
    }


    public async Task<IActionResult> ViewPurchasingAgentGroup()
    {
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //    return AccessDeniedView();

        //var product = await _productService.GetProductByIdAsync(productId)
        //              ?? throw new Exception($"No product found with the specified id {productId}");

        //var model = await _nexportPluginModelFactory.PrepareDuplicateNexportProductMappingModel(product);
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());
        return View($"{NexportDefaults.NexportPluginAdminViewBasePath}NexportWholesale/GroupProduct/List.cshtml", model);
    }


    public async Task<IActionResult> EditPurchasingAgentGroup()
    {
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
        //    return AccessDeniedView();

        //var product = await _productService.GetProductByIdAsync(productId)
        //              ?? throw new Exception($"No product found with the specified id {productId}");

        //var model = await _nexportPluginModelFactory.PrepareDuplicateNexportProductMappingModel(product);

        return View($"{NexportDefaults.NexportPluginAdminViewBasePath}NexportWholesale/PurchasingAgent/Edit.cshtml");
    }


    public async Task<IActionResult> DeletePurchasingAgentGroups()
    {
        //if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
        //    !await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportProductMapping))
        //    return AccessDeniedView();

        //if (selectedIds != null)
        //{
        //    foreach (var id in selectedIds)
        //    {
        //        var mapping = await _nexportService.GetProductMappingById(id);
        //        if (mapping != null)
        //        {
        //            await _nexportService.DeleteNexportProductMapping(mapping);

        //            var nopProduct = await _productService.GetProductByIdAsync(mapping.NopProductId);

        //            string storeText = "";

        //            if (mapping.StoreId.HasValue)
        //            {
        //                var store = await _storeService.GetStoreByIdAsync(mapping.StoreId.Value);
        //                if (store != null)
        //                    storeText = $"Store ID: {store.Id}, Name: {store.Name}";
        //            }


        //            //activity log
        //            await _customerActivityService.InsertActivityAsync(NexportDefaults.DELETE_NEXPORT_PRODUCT_MAPPING_ACTIVITY_LOG_TYPE,
        //                $"Deleted Nexport product mapping ({storeText}, Nexport Product ID: {mapping.NexportCatalogSyllabusLinkId}, Name: {mapping.NexportProductName}) in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})", mapping);

        //            var groupMembershipMappings =
        //                await _nexportService.GetProductGroupMembershipMappings(mapping.Id);
        //            foreach (var groupMembershipMapping in groupMembershipMappings)
        //            {
        //                await _nexportService.DeleteGroupMembershipMapping(groupMembershipMapping);

        //                //activity log
        //                await _customerActivityService.InsertActivityAsync(NexportDefaults.DELETE_NEXPORT_GROUP_MEMBERSHIP_MAPPING_ACTIVITY_LOG_TYPE,
        //                    $"Deleted Nexport group membership mapping (Group ID: {groupMembershipMapping.NexportGroupId}, Group Name: {groupMembershipMapping.NexportGroupName}) from product mapping (ID: {mapping.NexportCatalogSyllabusLinkId}, Name: {mapping.NexportProductName}) in product (ID: {nopProduct.Id}, Name: {nopProduct.Name})", mapping);
        //            }
        //        }
        //    }
        //}

        return Json(new { Result = true });
    }

}
