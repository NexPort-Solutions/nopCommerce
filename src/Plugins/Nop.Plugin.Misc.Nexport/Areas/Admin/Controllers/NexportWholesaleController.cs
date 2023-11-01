using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework.Mvc.Filters;

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
    private readonly ILogger _logger;

    public NexportWholesaleController(
        INexportPluginModelFactory nexportPluginModelFactory,
        IPermissionService permissionService,
        IStoreService storeService,
        INexportWholesaleService nexportWholesaleService,
        IWorkContext workContext, NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService productService,
        ILogger logger)
    {
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _permissionService = permissionService;
        _storeService = storeService;
        _nexportWholesaleService = nexportWholesaleService;
        _workContext = workContext;
        _nexportService = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _productService = productService;
        _logger = logger;
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

        var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProduct.cshtml";
        ViewData["ModelForPartialView"] = model;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/NexportGroups/List.cshtml");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid groupId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, currentCustomer);

        return Json(model);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesaleRedemptions))
            return await AccessDeniedDataTablesJson();

        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
        return Json(
            new { result = count }
        );
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
    {

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);
        //TODO @js - 
        //get invoice item by id
        //insert reset redemption queue item for the invoice item
        // return a status of "processing"
        if (invoiceItem != null)
        {
            try{

                invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.Processing;

                await _nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);

                await _nexportService.InsertNexportOrderInvoiceResetRedemptionQueueItem(
                    new NexportOrderInvoiceResetRedemptionQueueItem
                    {
                        OrderInvoiceItemId = invoiceItem.Id, UtcDateCreated = DateTime.UtcNow, RetryCount = 0
                    });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to insert invoice item {invoiceItem.InvoiceItemId} into the reset redemption queue",ex);
            }
        }

        return Json(
            new
            {
                result = invoiceItem!=null?invoiceItem.RedemptionStatus.GetDisplayName():"",
                //redirect = Url.RouteUrl("/")
            }
        );
    }
}
