using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;

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
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILogger _logger;
    private readonly NexportSettings _nexportSettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    public NexportWholesaleController(
        NexportSettings nexportSettings,
        INexportPluginModelFactory nexportPluginModelFactory,
        IPermissionService permissionService,
        IStoreService storeService,
        INexportWholesaleService nexportWholesaleService,
        IWorkContext workContext, NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService productService,
        IGenericAttributeService genericAttributeService,
        IStaticCacheManager staticCacheManager,
        INotificationService notificationService,
        ILocalizationService localizationService,
        ILogger logger)
    {
        _nexportSettings = nexportSettings;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _permissionService = permissionService;
        _storeService = storeService;
        _nexportWholesaleService = nexportWholesaleService;
        _workContext = workContext;
        _nexportService = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _productService = productService;
        _genericAttributeService = genericAttributeService;
        _staticCacheManager = staticCacheManager;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _logger = logger;
    }

    private async Task<bool> CheckWholesaleViewPermission()
    {
        if (await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return true;

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active)
            return false;

        var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
        if (nexportUserMapping == null)
            return false;

        var groupPermissionSearchCacheKey = new CacheKey("Misc.Nexport.SearchGroupForPermission.{0}-{1}",
            nexportUserMapping.NexportUserId.ToString(), _nexportSettings.RootOrganizationId.Value.ToString())
        {
            CacheTime = 30
        };

        var hasPermission = await _staticCacheManager.GetAsync(groupPermissionSearchCacheKey,
            async () => (await _nexportService.SearchGroupsForPermissionAsync(nexportUserMapping.NexportUserId, _nexportSettings.RootOrganizationId.Value)).Any());

        return hasPermission;
    }

    [Route("Admin/Wholesale/Create")]
    public virtual async Task<IActionResult> CreateWholesaleOrder()
    {
        var permissionResult = await CheckWholesaleViewPermission();
        if (!permissionResult)
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync(null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/CreateWholesaleOrder.cshtml", model);
    }

    [Route("Admin/Wholesale/PurchasingProductsList")]
    public virtual async Task<IActionResult> WholesaleOrderPurchasingProductsList(ICollection<int> selectedIds)
    {
        var permissionResult = await CheckWholesaleViewPermission();
        if (!permissionResult)
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderPurchasingProductsListAsync(selectedIds.ToList());

        return PartialView("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesaleOrderPurchasingProducts.cshtml", model);
    }

    [Route("Admin/Wholesale/AddProductsToWholesaleOrder")]
    public virtual async Task<IActionResult> AddProductToWholesaleOrder(int storeId)
    {
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderProductSearchModel(storeId);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/AddProductToWholesaleOrderPopup.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WholesaleOrderProductList(WholesaleOrderProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderProductListModelAsync(searchModel);

        return Json(model);
    }

    [Route("Admin/Wholesale/WholesaleOrderPaymentInfo")]
    public virtual async Task<IActionResult> WholesaleOrderPaymentInfo(string paymentSystemName)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareWholesaleOrderPaymentInfoModelAsync(paymentSystemName);

        return PartialView("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesaleOrderPaymentDetails.cshtml", model);
    }

    [HttpPost]
    [FormValueRequired("placewholesaleorder", "placewholesaleorder-continue")]
    [ParameterBasedOnFormName("placewholesaleorder-continue", "continueEditing")]
    public virtual async Task<IActionResult> PlaceWholesaleOrder(WholesaleOrderModel model, IFormCollection form, bool continueEditing)
    {
        //TODO: Check permissions

        var productIdsValue = JsonSerializer.Deserialize<int[]>(form["productIds"]);
        var customer = await _workContext.GetCurrentCustomerAsync();

        var store = await _storeService.GetStoreByIdAsync(model.StoreId);
        if (store == null || store.Deleted)
        {
            ModelState.AddModelError(string.Empty, "Invalid store.");
        }

        var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, model.StoreId);
        var paymentMethod = activePlugins
            .Select(plugin => plugin.ToPluginModel<PaymentMethodModel>())
            .FirstOrDefault(p => p.SystemName == model.PaymentMethod);

        if (paymentMethod is not { IsActive: true })
        {
            ModelState.AddModelError(string.Empty, "Selected payment method is invalid.");
        }

        //if (await _nexportService.GetOrganizationDetailsAsync(model.OrganizationId) is null)
        //{
        //    ModelState.AddModelError(nameof(model.OrganizationId), "Invalid organization.");
        //}
        //if (!model.IsRedemptionPeriodUnlimited && (model.RedeemByUtc is null || model.RedeemByUtc.Value <= DateTime.UtcNow))
        //{
        //    ModelState.AddModelError(nameof(model.RedeemByUtc), $"{nameof(model.RedeemByUtc)} must be a date in the future or {nameof(model.IsRedemptionPeriodUnlimited)} must be true.");
        //}
        //if (model.Quantity is > 100_000 or < 1)
        //{
        //    ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
        //}

        if (ModelState.IsValid)
        {
            var shoppingCartItems = new List<ShoppingCartItem>();

            foreach (var productId in productIdsValue)
            {
                var shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = ShoppingCartType.ShoppingCart,
                    StoreId = model.StoreId,
                    ProductId = productId,
                    AttributesXml = null,
                    Quantity = int.Parse(form[$"itemquantity{productId}"]),
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id,
                };

                shoppingCartItems.Add(shoppingCartItem);
            }

            var processingPaymentRequest = new ProcessPaymentRequest
            {
                OrderGuid = Guid.NewGuid(),
                OrderGuidGeneratedOnUtc = DateTime.UtcNow,
                StoreId = model.StoreId,
                CustomerId = customer.Id,
                PaymentMethodSystemName = model.PaymentMethod
            };

            //var placedOrderResult = await _nexportWholesaleService.PlaceWholesaleOrderAsync(processingPaymentRequest, shoppingCartItems);
            //if (!placedOrderResult.Success)
            //{
            //    throw new InvalidOperationException("Failed to place order");
            //}

            //var successStr = $"New bulk purchase has been placed. Click <a href=\"{Url.Action("Edit", "Order", new { id = placedOrderResult.PlacedOrder.Id })}\">here</a> to view the order details.";
            //_notificationService.SuccessNotification(successStr, false);

            //if (!continueEditing)
            //    return RedirectToAction("Edit", "Order", new { id = placedOrderResult.PlacedOrder.Id });

            return RedirectToAction("List", "Order");
        }

        model = await _nexportPluginModelFactory.PrepareWholesaleOrderModelAsync(model);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/CreateWholesaleOrder.cshtml", model);
    }

    //private async Task<WholesaleOrderValidationResult> ValidateWholesaleOrderAsync(WholesaleOrderModel model)
    //{
    //    var customer = await _workContext.GetCurrentCustomerAsync();
    //    var currentCustomerIsNotActive = customer is not { Active: true };
    //    var currentCustomerIsNotInNexport = await _nexportService.FindUserMappingByCustomerId(customer.Id) is null;
    //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders)
    //        || currentCustomerIsNotActive
    //        || currentCustomerIsNotInNexport)
    //    {
    //        ModelState.AddModelError("", "Current customer does not have permission for this resource.");
    //        return WholesaleOrderValidationResult.AccessDenied;
    //    }
    //    if (await _storeService.GetStoreByIdAsync(model.StoreId) is not { } store)
    //    {
    //        ModelState.AddModelError(nameof(model.StoreId), "Invalid store.");
    //    }
    //    else if (await GetPaymentMethodNameForStoreAndCustomerAsync(model.PaymentMethod, store, customer) is not { } paymentMethod)
    //    {
    //        ModelState.AddModelError(nameof(model.PaymentMethod), $"{nameof(model.PaymentMethod)} {model.PaymentMethod} is not valid for {nameof(model.StoreId)} {store.Name}.");
    //    }
    //    else if (await _nexportService.GetOrganizationDetailsAsync(model.OrganizationId) is null)
    //    {
    //        ModelState.AddModelError(nameof(model.OrganizationId), "Invalid organization.");
    //    }
    //    else if (!model.IsRedemptionPeriodUnlimited && (model.RedeemByUtc is null || model.RedeemByUtc.Value <= DateTime.UtcNow))
    //    {
    //        ModelState.AddModelError(nameof(model.RedeemByUtc), $"{nameof(model.RedeemByUtc)} must be a date in the future or {nameof(model.IsRedemptionPeriodUnlimited)} must be true.");
    //    }
    //    else if (model.Quantity is > 100_000 or < 1)
    //    {
    //        ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
    //    }
    //    else if (await _productService.GetProductByIdAsync(model.ProductId) is not { } product)
    //    {
    //        ModelState.AddModelError(nameof(model.Quantity), $"{nameof(model.Quantity)} {model.Quantity} is invalid.");
    //    }
    //    else if (ModelState.IsValid)
    //    {
    //        return WholesaleOrderValidationResult.Good(customer, store, paymentMethod, model.Quantity, product);
    //    }
    //    return WholesaleOrderValidationResult.Bad("Order parameters have invalid value: " + ModelState.ErrorCount + " errors.");
    //}

    [HttpsRequirement]
    public virtual async Task<IActionResult> AdminNexportGroups()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        var searchModel = new NexportGroupListSearchModel();
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroups.cshtml";
        ViewData["ModelForPartialView"] = searchModel;
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProducts(Guid groupId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);
        searchModel.HasGroupPermission = true;
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProductRedemptions(Guid? groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

        searchModel.HasGroupPermission = true;
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, currentCustomer);

        return Json(model);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return await AccessDeniedDataTablesJson();

        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
        return Json(
            new { result = count }
        );
    }

    public virtual async Task<IActionResult> GetMatchingUsers(CustomerStepModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return Challenge();

        var paged = new List<NexportUserModel>().ToPagedList(searchModel);

        if (searchModel.TableFirstDraw)
        {
            return Json(paged);
        }

        var customers = await _nexportService.SearchCustomersAsync(searchModel.SearchEmail ?? "");
        var nexportUsers = new List<NexportUserModel>();
        foreach (var customer in customers)
        {
            var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
            if (userMapping != null)
            {
                try
                {
                    var nexUser = await _nexportService.GetNexportUserAsync(userMapping.NexportUserId);
                    if (nexUser != null)
                    {
                        // populate name and email from nop customer so we don't get confused if the
                        // linked nexport account has a different name and email
                        nexportUsers.Add(new NexportUserModel
                        {
                            UserId = nexUser.UserId,
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Email = customer.Email,
                            OwnerOrgId = nexUser.OwnerOrgId,
                            OwnerOrg = nexUser.OwnerOrgId != null ? (await _nexportService.GetOrganizationDetailsAsync(nexUser.OwnerOrgId.Value))?.Name : "",
                            OwnerOrgShortName = nexUser.OwnerOrgShortName
                        });
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex);
                }

            }
        }

        paged = await nexportUsers.SelectAwait(async x => x).ToPagedListAsync(searchModel);

        var dtlist = new NexportUserListModel();

        dtlist = await dtlist.PrepareToGridAsync(searchModel, paged, () =>
        {
            return paged.SelectAwait(async x => x);
        });

        return Json(dtlist);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.UnassignInvoiceItem(invoiceItem);

        return Json(new { result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "" });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return AccessDeniedView();

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.CancelAwaitingInvoiceItem(invoiceItem);

        return Json(new { result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "" });
    }

    [HttpsRequirement]
    public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid? invoiceItemId, int? productId)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct.cshtml";
        ViewData["ModelForPartialView"] = model;

        //reset generic attributes for the redemption form
        var emailAddressAttribute = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
        if (emailAddressAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress", null);

        var sendViaEmailAttribute = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
        if (sendViaEmailAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", null);

        var selectedUserAttribute = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");
        if (selectedUserAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);

        var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
        if (firstName != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_FirstName", null);

        var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_lastName");
        if (lastName != null)
            await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_LastName", null);

        await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_ReturnUrl",
            Url.RouteUrl("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions") + "?groupId=" + groupId +
            "&productId=" + productId);

        await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId",
            invoiceItemId);

        await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_ProductId",
            productId);

        await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_OpenEndedProductMappingId",
            model.ProductMappingIdForOpenEndedProduct);

        await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);



        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }

    [HttpPost]
    public virtual async Task<IActionResult> AsnSaveCustomer(CustomerStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (model.SelectedUserId != null)
            {
                await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", false);
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", model.SelectedUserId.Value);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress", model.SelectedUserEmailAddress);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    //model is not valid. redisplay the form with validation message
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "email",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_CustomerStep.cshtml", model)
                        }
                    });
                }

                await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", true);
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress", model.Email);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_FirstName", model.FirstName);
                await _genericAttributeService.SaveAttributeAsync<string>(customer, "RedeemProductModel_LastName", model.LastName);
            }

            return await GoToTrainingStep(customer);
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    public async Task<IActionResult> GoToTrainingStep(Customer customer)
    {
        var productId =
            await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_productId");

        var invoiceItemId =
            await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId");

        var productStepModel = await _nexportPluginModelFactory.PrepareProductStepModel(productId, invoiceItemId);

        if (productStepModel.AvailableMappings.Count < 1)
            throw new Exception("Error preparing step. Could not load mappings for product");

        if (productStepModel.AvailableMappings.Count == 1)
        {
            return await GoToConfirmStep(productStepModel, customer);
        }

        return Json(new
        {
            update_section = new UpdateSectionJsonModel
            {
                name = "training",
                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductStep.cshtml", productStepModel)
            },
            goto_section = "training"
        });
    }

    public async Task<ActionResult> GoToConfirmStep(ProductStepModel model, Customer customer)
    {
        if (model.SelectedProductMappingId == null)
            throw new Exception("There was an error retreiving the product information from the previous steps");

        var productMapping = await _nexportService.GetProductMappingById(model.SelectedProductMappingId.Value);

        if (productMapping == null)
            throw new Exception("There was an error retrieving the product mapping information");

        var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);

        if (product == null)
            throw new Exception("There was an error retrieving the product");

        var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
        var email = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
        var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
        var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_LastName");

        return Json(new
        {
            update_section = new UpdateSectionJsonModel
            {
                name = "confirm",
                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ConfirmStep.cshtml", new ConfirmStepModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Product = product.Name,
                    SendViaEmail = (sendViaEmail != null && sendViaEmail.Value) ? "Email" : "Instant"
                })
            },
            goto_section = "confirm"
        });
    }


    [HttpPost]
    public virtual async Task<IActionResult> AsnSaveProduct(ProductStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (model.SelectedProductMappingId == null)
                throw new Exception("There was an error retreiving the product information from the previous steps");

            await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

            var productMapping = await _nexportService.GetProductMappingById(model.SelectedProductMappingId.Value);

            if (productMapping == null)
                throw new Exception("There was an error retrieving the product mapping information");

            var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);

            if (product == null)
                throw new Exception("There was an error retrieving the product");

            var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");

            var email = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
            var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_LastName");

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ConfirmStep.cshtml", new ConfirmStepModel
                    {
                        Email = email,
                        Product = product.Name,
                        FirstName = firstName,
                        LastName = lastName,
                        SendViaEmail = (sendViaEmail != null && sendViaEmail.Value) ? "Email" : "Instant"
                    })
                },
                goto_section = "confirm"
            });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    //[AutoValidateAntiforgeryToken]
    public virtual async Task<IActionResult> AsnSaveConfirm(ConfirmStepModel model, IFormCollection form)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var returnUrl = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_ReturnUrl");
            var selectedProductMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId");
            var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
            var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_LastName");
            var openEndedMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_OpenEndedProductMappingId");
            var emailAddress = await _genericAttributeService.GetAttributeAsync<string>(customer, "RedeemProductModel_EmailAddress");
            var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId");
            var selectedUserId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");

            if (invoiceItemId == null)
                throw new Exception("error retreiving invoice item id for transaction.");


            bool success = await _nexportService.RedeemProductForCustomer(new RedeemProductModel
            {
                ReturnUrl = returnUrl ?? "",
                SelectedProductMappingId = selectedProductMappingId,
                AssignmentType = sendViaEmail != null && sendViaEmail.Value ? "Email" : "Instant",
                Email = emailAddress,
                FirstName = firstName,
                LastName = lastName,
                ProductMappingIdForOpenEndedProduct = openEndedMappingId,
                InvoiceItemId = invoiceItemId.Value,
                UserId = selectedUserId
            });

            return Json(new { success = 1 });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpGet]
    [Route("Admin/Wholesale/FundingPool/List")]
    public async Task<IActionResult> ListFundingPools()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolSearchModelAsync(new NexportFundingPoolSearchModel());

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/List.cshtml", model);
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    [Route("Admin/Wholesale/FundingPool/List")]
    public async Task<IActionResult> ListFundingPools(NexportFundingPoolSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolListModelAsync(searchModel);

        return Json(model);
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [Route("Admin/Wholesale/FundingPool/Create")]
    public async Task<IActionResult> CreateFundingPool()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(new NexportFundingPoolModel(), null);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Create.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [AutoValidateAntiforgeryToken]
    [Route("Admin/Wholesale/FundingPool/Create")]
    public async Task<IActionResult> CreateFundingPool(NexportFundingPoolModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var fundingPool = model.ToEntity<NexportFundingPool>();
            fundingPool.UtcDateCreated = DateTime.UtcNow;

            await _nexportWholesaleService.InsertFundingPool(fundingPool);

            _notificationService.SuccessNotification("A new funding pool has been added.");

            if (!continueEditing)
                return RedirectToAction("ListFundingPools", "NexportWholesale");

            return RedirectToAction("EditFundingPool", "NexportWholesale", new { id = fundingPool.Id });
        }

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Create.cshtml", model);
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [Route("Admin/Wholesale/FundingPool/Edit/{id}")]
    public async Task<IActionResult> EditFundingPool(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        var model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(null, fundingPool);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Edit.cshtml", model);
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [Route("Admin/Wholesale/FundingPool/Edit/{id}")]
    public virtual async Task<IActionResult> EditFundingPool(NexportFundingPoolModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(model.Id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        if (ModelState.IsValid)
        {
            fundingPool = model.ToEntity(fundingPool);
            await _nexportWholesaleService.UpdateFundingPool(fundingPool);

            _notificationService.SuccessNotification("Successfully update funding pool");

            if (!continueEditing)
                return RedirectToAction("ListFundingPools", "NexportWholesale");

            return RedirectToAction("EditFundingPool", "NexportWholesale", new { id = fundingPool.Id });
        }

        model = await _nexportPluginModelFactory.PrepareNexportFundingPoolModelAsync(model, fundingPool);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/FundingPool/Edit.cshtml", model);
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    [Route("Admin/Wholesale/FundingPool/Delete/{id}")]
    public virtual async Task<IActionResult> DeleteFundingPool(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        var fundingPool = await _nexportWholesaleService.GetFundingPoolById(id);
        if (fundingPool == null)
            return RedirectToAction("ListFundingPools", "NexportWholesale");

        await _nexportWholesaleService.DeleteFundingPool(fundingPool);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.FundingPool.Deleted"));

        return RedirectToAction("ListFundingPools", "NexportWholesale");
    }

    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedFundingPools(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _nexportWholesaleService.DeleteFundingPools(await _nexportWholesaleService.GetFundingPoolByIds(selectedIds.ToArray()));

        return Json(new { Result = true });
    }
}