using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.Nexport.Controllers;
using Nop.Plugin.Misc.Nexport.Extensions;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using System.Globalization;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class WholesaleController : BaseAdminController, IWholesaleController
{
    private readonly IPermissionService _permission;
    private readonly IStoreService _store;
    private readonly IWholesaleService _wholesale;
    private readonly IWorkContext _workContext;
    private readonly IUserMappingService _userMapping;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _product;
    private readonly Settings _settings;
    private readonly ICustomerService _customer;
    private readonly IPluginModelFactory _model;
    private readonly IOrganizationService _organization;
    private readonly IGroupService _group;

    public WholesaleController(
        IPermissionService permission,
        IStoreService store,
        IWholesaleService wholesale,
        IWorkContext workContext,
        IPaymentPluginManager paymentPluginManager,
        IProductService product,
        ICustomerService customer,
        IPluginModelFactory pluginModelFactory,
        Settings settings,
        IOrganizationService organization,
        IGroupService group,
        IUserMappingService userMapping)
    {
        _permission = permission;
        _store = store;
        _wholesale = wholesale;
        _workContext = workContext;
        _paymentPluginManager = paymentPluginManager;
        _product = product;
        _customer = customer;
        _model = pluginModelFactory;
        _settings = settings;
        _organization = organization;
        _group = group;
        _userMapping = userMapping;
    }

    [HttpGet]
    public virtual async Task<IActionResult> Create()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            return AccessDeniedView();
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active || await _userMapping.FindByCustomerId(customer.Id) is null)
        {
            return AccessDeniedView();
        }
        return View();
    }

    [HttpPost]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleCreateModel model)
    {
        var customer = await _customer.GetCustomerByIdAsync(model.AgentId);
        var currentCustomerIsNotInNexport = await _userMapping.FindByCustomerId(customer.Id) is null;
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
            || currentCustomerIsNotInNexport)
        {
            return AccessDeniedView();
        }
        var store = await _store.GetStoreByIdAsync(model.StoreId);
        var paymentMethod = await GetPaymentMethodNameForStoreAndCustomerAsync(model.PaymentMethod, store, customer);
        var product = await _product.GetProductByIdAsync(model.ProductId);
        var group = await _organization.GetOrganizationDetails(model.GroupId)
            ?? throw new("Group not found");
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
        var placedOrderResult = await _wholesale.PlaceWholesaleOrderAsync(request, shoppingCartItems, customer, group, model.RedeemByUtc);
        if (!placedOrderResult.Success)
        {
            // change to _notification.NotifyError
            throw new("Failed to place order");
        }
        return View("Redirect", new NextportWholesaleOrderCreationResponse { Id = placedOrderResult.PlacedOrder.Id });
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchPurchasingAgents(string term)
    {
        var purchasingAgents = (await _customer.GetAllCustomersAsync())
            .WhereAwait(customerToUser)
            .Select(customerToJQueryObject);
        return Json(purchasingAgents);

        async ValueTask<bool> customerToUser(Customer customer)
            => await _userMapping.FindByCustomerId(customer.Id) is not null;

        static JQueryObject<string> customerToJQueryObject(Customer customer)
            => new($"{customer.FirstName} {customer.LastName}".Trim(), customer.Id.ToString(CultureInfo.InvariantCulture));
    }

    [HttpGet]
    public virtual async Task<IActionResult> SearchOrganizations()
    {
        if (!_settings.IsValid())
        {
            throw new("Nexport Settings are misconfigured.");
        }
        var root = _settings.RootOrganizationId.Value;
        var organizations = (await _organization.FindAllOrganizations(root))
            .Select(organizationToJQueryObject);
        return Json(organizations);

        static JQueryObject<string> organizationToJQueryObject(OrganizationResponseItem organization)
            => new($"{organization.ShortName} ({organization.Name})", organization.OrgId.ToString());
    }

    [HttpGet]
    public virtual async Task<IActionResult> AdminGroups()
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return AccessDeniedView();
        }
        var model = new GroupListSearchModel { AdminView = true };
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/Groups.cshtml";
        ViewData["ModelForPartialView"] = model;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> GroupProductsAsync(Guid groupId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return AccessDeniedView();
        }
        var model = await _model.GroupProductListSearchModel(groupId, true);
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/GroupProducts.cshtml";
        ViewData["ModelForPartialView"] = model;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> GroupProductRedemptions(Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return AccessDeniedView();
        }
        var model = await _model.GroupProductRedemptionListSearchModel(groupId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/GroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = model;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> RedeemProduct(Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return AccessDeniedView();
        }
        var model = await _model.RedeemProductOrModifyProductRedemptionModel(groupId, productId, true);
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> ModifyRedemption(Guid groupId, int productId, Guid invoiceItemId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return AccessDeniedView();
        }
        var model = await _model.RedeemProductOrModifyProductRedemptionModel(groupId, productId, true, invoiceItemId);
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProductOrModifyProductRedemption.cshtml";
        ViewData["ModelForPartialView"] = model;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/Wholesale/Groups/List.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GetGroups(GroupListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.GroupListModel(model));
    }

    [HttpPost]
    public async Task<IActionResult> GetGroupProducts(GroupProductListSearchModel model, Guid groupId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.GroupProductListModel(model, groupId));
    }

    [HttpPost]
    public async Task<IActionResult> GetGroupProductRedemptions(GroupProductRedemptionListSearchModel model, Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return await AccessDeniedDataTablesJson();
        }
        return Json(await _model.GroupProductCustomerListModel(model, groupId, productId));
    }

    [HttpPost]
    public async Task<IActionResult> GetAvailableGroupProductRedemptionsCount(GroupProductRedemptionListSearchModel _, Guid groupId, int productId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageWholesaleRedemptions))
        {
            return await AccessDeniedDataTablesJson();
        }
        var count = await _group.GetAvailableGroupProductRedemptionsCount(groupId, productId);
        return Json(new { Result = count });
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
}
