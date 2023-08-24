using Microsoft.AspNetCore.Mvc;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportWholesaleController : BaseAdminController
{
    private readonly IPermissionService _permissionService;
    private readonly IStoreService _storeService;
    private readonly INexportWholesaleService _nexportWholesaleService;
    private readonly IWorkContext _workContext;
    private readonly NexportService _nexportService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _productService;
    private readonly NexportSettings _nexportSettings;
    private ICustomerService _customerService;

    public WholesaleController(
        IPermissionService permissionService,
        IStoreService storeService,
        INexportWholesaleService nexportWholesaleService,
        IWorkContext workContext,
        NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService productService,
        ICustomerService customerService,
        NexportSettings nexportSettings)
    {
        _permissionService = permissionService;
        _storeService = storeService;
        _nexportWholesaleService = nexportWholesaleService;
        _workContext = workContext;
        _nexportService = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _productService = productService;
        _customerService = customerService;
        _nexportSettings = nexportSettings;
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
        return View();
    }

    [HttpPost]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleCreateModel model)
    {
        var validation = await ValidateWholesaleOrderAsync(model);
        return validation switch
        {
            WholesaleOrderValidationResult.AccessDeniedResult => AccessDeniedView(),
            WholesaleOrderValidationResult.BadResult bad => BadRequest(bad.Error),
            WholesaleOrderValidationResult.GoodResult result => await HandleGoodWholesaleOrderAsync(result),
            _ => throw new NotImplementedException()
        };
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchPurchasingAgents(string term)
    {
        var purchasingAgents = (await _customerService.GetAllCustomersAsync())
            .WhereAwait(customerToNexportUser)
            .Select(customerToJQueryObject);
        return Json(purchasingAgents);

        async ValueTask<bool> customerToNexportUser(Customer customer) => await _nexportService.FindUserMappingByCustomerId(customer.Id) is not null;
        static JQueryObject customerToJQueryObject(Customer customer) => new($"{customer.FirstName} {customer.LastName}".Trim(), customer.Id.ToString());
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchOrganizations()
    {
        var root = _nexportSettings.RootOrganizationId.Value;
        var organizations = (await _nexportService.FindAllOrganizationsAsync(root))
            .Select(organizationToJQueryObject);
        return Json(organizations);

        static JQueryObject organizationToJQueryObject(OrganizationResponseItem organization)
            => new(organization.ShortName + $" ({organization.Name})", organization.OrgId.ToString());
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
        var placedOrderResult = await _nexportWholesaleService.PlaceWholesaleOrderAsync(processingPaymentRequest, shoppingCartItems, result.Customer, result.Group, result.RedeemByUtc);
        if (!placedOrderResult.Success)
        {
            throw new InvalidOperationException("Failed to place order");
        }
        return View("Redirect", new NextportWholesaleOrderCreationResponse { Id = placedOrderResult.PlacedOrder.Id });
    }

    private async Task<WholesaleOrderValidationResult> ValidateWholesaleOrderAsync(WholesaleCreateModel model)
    {
        var customer = await _customerService.GetCustomerByIdAsync(model.AgentId);
        var currentCustomerIsNotInNexport = await _nexportService.FindUserMappingByCustomerId(customer.Id) is null;
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
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
        else if (await _nexportService.GetOrganizationDetailsAsync(model.GroupId) is null)
        {
            ModelState.AddModelError(nameof(model.GroupId), "Invalid group.");
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
        else if (await _nexportService.GetOrganizationDetailsAsync(model.GroupId) is not { } group)
        {
            ModelState.AddModelError(nameof(model.GroupId), $"{nameof(model.GroupId)} {model.GroupId} is invalid.");
        }
        else if (ModelState.IsValid)
        {
            return WholesaleOrderValidationResult.Good(store, product, model.RedeemByUtc, customer, group, model.Quantity, paymentMethod);
        }
        return WholesaleOrderValidationResult.Bad("Order parameters have invalid value: " + ModelState.ErrorCount + " errors.");
    }

    private abstract record WholesaleOrderValidationResult
    {
        public static GoodResult Good(Store store, Product product, DateTime? redeemByUtc, Customer customer, OrganizationResponseItem group, int quantity, string paymentMethod)
            => new(store, product, redeemByUtc, customer, group, quantity, paymentMethod);
        public static readonly AccessDeniedResult AccessDenied = new();
        public static BadResult Bad(string? error) => new(error);

        public record GoodResult(Store Store, Product Product, DateTime? RedeemByUtc, Customer Customer, OrganizationResponseItem Group, int Quantity, string PaymentMethod) : WholesaleOrderValidationResult;
        public record AccessDeniedResult : WholesaleOrderValidationResult;

        public record BadResult(string? Error) : WholesaleOrderValidationResult;
    };

    [HttpsRequirement]
    public virtual async Task<IActionResult> AdminNexportGroups()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = new NexportGroupListSearchModel();
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
        ViewData["ModelForPartialView"] = searchModel;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProducts(Guid groupId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProductRedemptions(Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> RedeemProduct(Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductOrModifyProductRedemptionModel(groupId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> ModifyRedemption(Guid groupId, int productId, Guid invoiceItemId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductOrModifyProductRedemptionModel(groupId, productId, invoiceItemId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid groupId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId);

        return Json(model);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductCustomerListModelAsync(searchModel, groupId, productId);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCount(groupId, productId);
        return Json(
            new { result = count }
        );
    }
    private async Task<string?> GetPaymentMethodNameForStoreAndCustomerAsync(string paymentMethodSystemName, Store store, Customer customer)
    {
        var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id);
        var paymentPluginName = activePlugins
            .Select(plugin => plugin.ToPluginModel<PaymentMethodModel>().SystemName)
            .FirstOrDefault(systemName => systemName == paymentMethodSystemName);
        return paymentPluginName;
    }
}
