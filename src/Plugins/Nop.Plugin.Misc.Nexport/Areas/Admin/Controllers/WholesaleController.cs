using System.Globalization;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Infrastructure;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class WholesaleController : BaseAdminController
{
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IPermissionService _permission;
    private readonly IStoreService _store;
    private readonly IWholesaleService _wholesale;
    private readonly IWorkContext _workContext;
    private readonly NexportService _nexport;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _product;
    private readonly ICustomerService _customer;
    private readonly INotificationService _notification;
    private readonly NexportSettings _settings;
    private readonly IProductService _productService;

    public WholesaleController(
        INexportPluginModelFactory nexportModelFactory,
        IPermissionService permission,
        IStoreService store,
        IWholesaleService wholesale,
        IWorkContext workContext,
        NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService product,
        ICustomerService customer,
        INotificationService notification,
        NexportSettings settings,
        IProductService productService)
    {
        _nexportPluginModelFactory = nexportModelFactory;
        _permission = permission;
        _store = store;
        _wholesale = wholesale;
        _workContext = workContext;
        _nexport = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _product = product;
        _customer = customer;
        _notification = notification;
        _settings = settings;
        _productService = productService;
    }

    public virtual async Task<IActionResult> Create(int? productId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return AccessDeniedView();
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active || await _nexport.FindUserMappingByCustomerId(customer.Id) is null)
        {
            return AccessDeniedView();
        }
        if (productId is not null)
        {
            var model = new WholesaleCreateModel { ProductId = productId.Value };
            return View(model);
        }
        return View();
    }

    [HttpPost]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleCreateModel model)
    {
        var customer = await _customer.GetCustomerByIdAsync(model.AgentId);
        var currentCustomerIsNotInNexport = await _nexport.FindUserMappingByCustomerId(customer.Id) is null;
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
            || currentCustomerIsNotInNexport)
        {
            return AccessDeniedView();
        }
        var store = await _store.GetStoreByIdAsync(model.StoreId);
        var paymentMethod = await GetPaymentMethodNameForStoreAndCustomerAsync(model.PaymentMethod, store, customer);
        var product = await _product.GetProductByIdAsync(model.ProductId);
        if (await _nexport.GetOrganizationDetailsAsync(model.GroupId) is not { } group)
        {
            return BadRequest($"Ground {model.GroupId} not found");
        }
        var shoppingCartItem = new ShoppingCartItem
        {
            ShoppingCartType = ShoppingCartType.ShoppingCart,
            StoreId = store.Id,
            ProductId = product.Id,
            AttributesXml = null,
            CustomerEnteredPrice = decimal.Zero,
            Quantity = model.Quantity,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
            CustomerId = customer.Id,
        };
        var shoppingCartItems = new List<ShoppingCartItem> { shoppingCartItem };
        var request = new ProcessPaymentRequest
        {
            OrderGuid = Guid.NewGuid(),
            OrderGuidGeneratedOnUtc = DateTime.UtcNow,
            StoreId = store.Id,
            CustomerId = customer.Id,
            PaymentMethodSystemName = paymentMethod,
        };
        var placedOrderResult = await _wholesale.PlaceWholesaleOrderAsync(request, shoppingCartItems, customer, group, model.RedeemByUtc, model.FundingPoolId);
        if (!placedOrderResult.Success)
        {
            await _notification.ErrorNotificationAsync(new FailedToPlaceWholesaleOrderException("Failed to place order"));
        }
        return View(nameof(Redirect), new WholesaleOrderCreationResponse { Id = placedOrderResult.PlacedOrder.Id });
    }

    private async Task<string?> GetPaymentMethodNameForStoreAndCustomerAsync(string? paymentMethodSystemName, Store store, Customer customer)
    {
        if (paymentMethodSystemName is null)
        {
            return null;
        }
        var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id);
        return activePlugins
            .Select(plugin => plugin.ToPluginModel<PaymentMethodModel>().SystemName)
            .FirstOrDefault(systemName => systemName == paymentMethodSystemName);
    }

    [HttpsRequirement]
    public virtual async Task<IActionResult> AdminNexportGroups()
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = new NexportGroupListSearchModel();
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
        ViewData["ModelForPartialView"] = searchModel;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProducts(Guid groupId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProductRedemptions(Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> RedeemProduct(Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductOrModifyProductRedemptionModel(groupId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> ModifyRedemption(Guid groupId, int productId, Guid invoiceItemId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductOrModifyProductRedemptionModel(groupId, productId, invoiceItemId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid groupId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId);

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId);

        return Json(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var count = await _nexport.GetAvailableNexportGroupProductRedemptionsCount(groupId, productId);
        return Json(
            new { result = count }
        );
    }

    [HttpGet]
    public async Task<IActionResult> SearchProducts(string term)
    {

        var stores = await _store.GetAllStoresAsync();
        var products = await stores.SelectManyAwait(store => _nexport.GetProductMappingsByStoreId(store.Id))
            .Select(mapping => mapping.DisplayName is { } name && mapping.NopProductId is { } id
                ? new { label = name, value = id.ToString(CultureInfo.InvariantCulture) }
                : null)
            .Where(product => product?.label.Contains(term, StringComparison.OrdinalIgnoreCase) is true)
            .Take(10)
            .ToListAsync();
        return Json(products);
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchPurchasingAgents(string term)
    {
        var purchasingAgents = (await _customer.GetAllCustomersAsync())
            .WhereAwait(customerIsUser)
            .Select(customer => new { label = $"{customer.FirstName} {customer.LastName}".Trim(), value = customer.Id.ToString(CultureInfo.InvariantCulture) });
        return Json(purchasingAgents);

        async ValueTask<bool> customerIsUser(Customer customer)
            => await _nexport.FindUserMappingByCustomerId(customer.Id) is not null;
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchOrganizations()
    {
        if (_settings.RootOrganizationId is not { } root)
        {
            throw new("Nexport Settings are misconfigured.");
        }
        var organizations = (await _nexport.FindAllOrganizationsAsync(root))
            .Select(organization => new { label = $"{organization.ShortName} ({organization.Name})", value = organization.OrgId.ToString() });
        return Json(organizations);
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchStores(string term)
    {
        var stores = (await _store.GetAllStoresAsync())
            .Where(store => store.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(store => new { label = store.Name, value = store.Id.ToString(CultureInfo.InvariantCulture) });
        return Json(stores);
    }
}
