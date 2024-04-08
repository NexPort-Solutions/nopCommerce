using System.ServiceModel.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;
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

    public NexportWholesaleController(
        INexportPluginModelFactory nexportPluginModelFactory,
        IPermissionService permissionService,
        IStoreService storeService,
        INexportWholesaleService nexportWholesaleService,
        IWorkContext workContext, NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService productService,
        IGenericAttributeService genericAttributeService,
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
        _genericAttributeService = genericAttributeService;
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
    public async Task<IActionResult> AdminNexportGroupProducts()
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync();
        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> AdminNexportGroupProductRedemptions(Guid? groupId, int productId)
    {
        var searchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

        searchModel.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = searchModel;

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/NexportWholesale/WholesalePurchases/NexportGroups/List.cshtml");
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, currentCustomer);

        return Json(model);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
    {
        var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
        return Json(
            new { result = count }
        );
    }

    public virtual async Task<IActionResult> GetMatchingUsers(CustomerStepModel searchModel)
    {
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
                            OwnerOrg = nexUser.OwnerOrgId != null
                                ? (await _nexportService.GetOrganizationDetailsAsync(nexUser.OwnerOrgId.Value))
                                ?.Name
                                : "",
                            OwnerOrgShortName = nexUser.OwnerOrgShortName
                        });
                    }
                }
                catch(Exception ex)
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
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.UnassignInvoiceItem(invoiceItem);

        return Json(
            new
            {
                result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
            }
        );
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
    {
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

        if (invoiceItem != null)
            await _nexportService.CancelAwaitingInvoiceItem(invoiceItem);

        return Json(
            new
            {
                result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
            }
        );
    }

    [HttpsRequirement]
    public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid? invoiceItemId, int? productId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
        model.AdminView = true;
        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct.cshtml";
        ViewData["ModelForPartialView"] = model;

        //reset generic attributes for the redemption form
        var emailAddressAttribute = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
        if (emailAddressAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", null);

        var sendViaEmailAttribute = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
        if (sendViaEmailAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", null);

        var selectedUserAttribute = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");
        if (selectedUserAttribute != null)
            await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);

        var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
        if (firstName != null)
            await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", null);

        var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_lastName");
        if (lastName != null)
            await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", null);

        await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_ReturnUrl",
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
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", model.SelectedUserEmailAddress);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);
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
                            name = "customer",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_CustomerStep.cshtml", model)
                        }
                    });
                }

                await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", true);
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", model.Email);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.FirstName);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.LastName);
            }

            return await GoToProductStep(customer);
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = exc.Message });
        }
    }

    public async Task<IActionResult> GoToProductStep(Customer customer)
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
                name = "product",
                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductStep.cshtml", productStepModel)
            },
            goto_section = "product"
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
        var email = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
        var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
        var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");

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

            var email = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
            var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");

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
            var returnUrl = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_ReturnUrl");
            var selectedProductMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId");
            var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
            var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");
            var openEndedMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_OpenEndedProductMappingId");
            var emailAddress = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
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
}
