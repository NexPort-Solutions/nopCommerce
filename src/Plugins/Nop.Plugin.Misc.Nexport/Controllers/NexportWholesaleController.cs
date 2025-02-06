using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public class NexportWholesaleController : BasePluginController
{
    #region Fields

    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerService _customerService;
    private readonly NexportService _nexportService;
    private readonly IPermissionService _permissionService;
    private readonly NexportSettings _nexportSettings;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;
    private readonly LocalizationSettings _localizationSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IReturnRequestService _returnRequestService;
    protected readonly ICustomNumberFormatter _customNumberFormatter;
    protected readonly IWorkflowMessageService _workflowMessageService;

    #endregion

    #region Constructor

    public NexportWholesaleController(
        INexportPluginModelFactory nexportPluginModelFactory,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ICustomerService customerService,
        NexportService nexportService,
        IPermissionService permissionService,
        NexportSettings nexportSettings,
        IProductService productService,
        IOrderService orderService,
        ILocalizationService localizationService,
        ILogger logger,
        LocalizationSettings localizationSettings,
        IReturnRequestService returnRequestService,
        ICustomNumberFormatter customNumberFormatter,
        IWorkflowMessageService workflowMessageService
    )
    {
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _storeContext = storeContext;
        _customerService = customerService;
        _nexportService = nexportService;
        _permissionService = permissionService;
        _nexportSettings = nexportSettings;
        _productService = productService;
        _orderService = orderService;
        _localizationService = localizationService;
        _logger = logger;
        _localizationSettings = localizationSettings;
        _returnRequestService = returnRequestService;
        _customNumberFormatter = customNumberFormatter;
        _workflowMessageService = workflowMessageService;
    }

    #endregion

    #region Actions

    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> SetPurchaseGroupForCustomer(string groupSelected)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-PurchasingGroup", groupSelected, store.Id);

        return Json(new { Result = true });
    }

    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> RemovePurchaseGroupForCustomer()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        // Remove purchasing group attribute
        await _genericAttributeService.SaveAttributeAsync<string>(customer, "WholesaleOrder-PurchasingGroup", null, store.Id);

        return Json(new { Result = true });
    }

    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> SetPurchaseFundingPoolForCustomer(string fundingPoolId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-FundingPoolId", fundingPoolId, store.Id);

        return Json(new { Result = true });
    }

    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> RemovePurchaseFundingPoolForCustomer()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        // Remove purchasing funding pool attribute
        await _genericAttributeService.SaveAttributeAsync<string>(customer, "WholesaleOrder-FundingPoolId", null, store.Id);

        return Json(new { Result = true });
    }

    [AutoValidateAntiforgeryToken]
    [HttpPost]
    public async Task<IActionResult> SetPurchaseRedeemByForCustomer(string utcRedeemByDate)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        await _genericAttributeService.SaveAttributeAsync(customer, "WholesaleOrder-RedeemBy", utcRedeemByDate, store.Id);

        return Json(new { Result = true });
    }

    [HttpsRequirement]
    public async Task<IActionResult> CustomerNexportGroupProducts(int? productId = null, int? statusId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(customer.Id, productId, statusId);

        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
        ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> CustomerNexportGroupProductRedemptions(int productId, Guid? groupId, int? orderId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId, orderId);

        ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProductRedemptions.cshtml";
        ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, customer);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, int? orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return await AccessDeniedDataTablesJson();

        var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, customer, orderId);

        return Json(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(
        NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, int? orderId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        int? count;

        if (groupId == null)
        {
            var isAdmin = await _customerService.IsAdminAsync(customer);
            if (searchModel.AdminView && isAdmin)
            {
                //show all items under group not assigned
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(null, productId, orderId);
            }
            else
            {
                //show only items for group not assigned that belong to the current store and current customer
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(null, productId, orderId, customer, store);
            }
        }
        else
        {
            var hasGroupPermission = await _nexportService.HasGroupPermissionAsync(customer, groupId.Value);
            if (searchModel.AdminView && hasGroupPermission)
            {
                //show all items for the group
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId, orderId);
            }
            else
            {
                //show only items for the group that belong to the current store
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId, orderId, store: store);
            }
        }

        return Json(new { result = count });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

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

    public virtual async Task<IActionResult> GetMatchingUsers(CustomerStepModel searchModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currentCustomer))
            return Challenge();

        var paged = new List<NexportUserAssignmentModel>().ToPagedList(searchModel);

        if (searchModel.TableFirstDraw)
        {
            return Json(paged);
        }

        var customers = await _nexportService.SearchCustomersAsync(searchModel.SearchEmail ?? "");
        var nexportUsers = new List<NexportUserAssignmentModel>();
        foreach (var customer in customers)
        {
            var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
            if (userMapping != null)
            {
                var invoiceRedemptions = await _nexportService.SearchGroupProductRedemptionsAsync(
                    searchModel.GroupId, searchModel.ProductId,
                    customerEmail: customer.Email,
                    redemptionStatus: NexportOrderInvoiceItemRedemptionStatus.Assigned);

                try
                {
                    var nexUser = await _nexportService.GetNexportUserAsync(userMapping.NexportUserId);

                    if (nexUser != null)
                    {
                        // populate name and email from nop customer so we don't get confused if the
                        // linked nexport account has a different name and email
                        nexportUsers.Add(new NexportUserAssignmentModel
                        {
                            UserId = nexUser.UserId,
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Email = customer.Email,
                            OwnerOrgId = nexUser.OwnerOrgId,
                            OwnerOrg = nexUser.OwnerOrgId != null
                                ? (await _nexportService.GetOrganizationDetailsAsync(nexUser.OwnerOrgId.Value))?.Name
                                : "",
                            OwnerOrgShortName = nexUser.OwnerOrgShortName,
                            IsAvailable = !invoiceRedemptions.Any()
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

        var dtlist = new NexportUserAssignmentListModel();

        dtlist = await dtlist.PrepareToGridAsync(searchModel, paged, () =>
        {
            return paged.SelectAwait(async x => x);
        });

        return Json(dtlist);
    }

    [HttpsRequirement]
    public virtual async Task<IActionResult> RedeemByEmail(int invoiceItemId, string email, int productMappingId)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currentCustomer))
            return Challenge();
        //if (currentCustomer.Email!=email)
        //    return Challenge();

        var model = await _nexportPluginModelFactory.PrepareRedeemByEmailModel(invoiceItemId,
            email, productMappingId);
        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemByEmail.cshtml", model);
    }

    [HttpsRequirement]
    public virtual async Task<IActionResult> RedeemAwaitingInvoiceItem(int invoiceItemId, Guid nexportUserId, int productMappingId)
    {
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(invoiceItemId);
        if (invoiceItem != null)
            await _nexportService.RedeemAwaitingInvoiceItem(invoiceItem, nexportUserId, productMappingId);

        return Json(new
        {
            Result = true
        });
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

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
    public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid invoiceItemId, int productId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
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

        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_ReturnUrl",
            Url.RouteUrl("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions") + "?groupId=" + groupId +
            "&productId=" + productId);

        await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId",
            invoiceItemId);

        await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_ProductId",
            productId);

        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_OpenEndedProductMappingId",
            model.ProductMappingIdForOpenEndedProduct);

        await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
    }

    [HttpsRequirement]
    public async Task<IActionResult> RequestUnassignment(Guid? groupId, Guid? invoiceItemId, int? productId, int? customerId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var model = await _nexportPluginModelFactory.PrepareSubmitUnassignmentRequestModel(groupId, invoiceItemId, productId, customerId);

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestUnassignment.cshtml", model);
    }

    [HttpPost, ActionName("RequestUnassignment")]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> UnassignmentRequestSubmit(SubmitRedemptionUnassignmentRequestModel model, IFormCollection form)
    {
        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestUnassignment.cshtml", model);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var requestReason = await _nexportService.GetNexportRedemptionUnassignmentRequestReasonByIdAsync(model.RedemptionUnassignmentRequestReasonId);

        var unassignmentRequest = new NexportRedemptionUnassignmentRequest
        {
            InvoiceItemId = model.InvoiceItemId,
            RequestedByCustomerId = currentCustomer.Id,
            CustomerComments = model.Comments,
            RequestStatus = NexportRedemptionUnassignmentRequestStatus.Received,
            ReasonForUnassignment = requestReason != null
                ? await _localizationService.GetLocalizedAsync(requestReason, x => x.Name)
                : "not available",
            StaffNotes = string.Empty,
            UtcCreatedDate = DateTime.UtcNow,
            UtcLastModifiedDate = DateTime.UtcNow
        };

        await _nexportService.InsertRedemptionUnassignmentRequestAsync(unassignmentRequest);

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);

        await _nexportService.SendNewRedemptionUnassignmentRequestStoreOwnerNotificationAsync(
            unassignmentRequest, invoiceItem, _localizationSettings.DefaultAdminLanguageId);

        await _nexportService.SendNewRedemptionUnassignmentRequestCustomerNotificationAsync(unassignmentRequest, invoiceItem);

        model = await _nexportPluginModelFactory.PrepareSubmitRedemptionUnassignmentRequestModelAsync(model);
        model.Result = await _localizationService.GetResourceAsync("RedemptionUnassignmentRequests.Submitted");

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestUnassignment.cshtml", model);
    }

    [HttpsRequirement]
    public async Task<IActionResult> RequestRefund(Guid invoiceItemId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var model = await _nexportPluginModelFactory.PrepareInvoiceItemRefundRequestModel(invoiceItemId);

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestRefund.cshtml", model);
    }

    [HttpPost, ActionName("RequestRefund")]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> InvoiceItemRefundRequestSubmit(SubmitInvoiceItemRefundRequestModel model, IFormCollection form)
    {
        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestRefund.cshtml", model);

        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(model.InvoiceItemId);

        var order = await _orderService.GetOrderByIdAsync(invoiceItem.OrderId);
        var orderItem = await _orderService.GetOrderItemByIdAsync(invoiceItem.OrderItemId);

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        var requestActions = await _returnRequestService.GetAllReturnRequestActionsAsync();
        var requestReasons = await _returnRequestService.GetAllReturnRequestReasonsAsync();

        var rrr = requestReasons.FirstOrDefault(x => x.Name.Contains("No longer need"));
        var rra = requestActions.FirstOrDefault(x => x.Name.Contains("Refund"));

        var store = await _storeContext.GetCurrentStoreAsync();

        var rr = new ReturnRequest
        {
            CustomNumber = "",
            StoreId = store.Id,
            OrderItemId = orderItem.Id,
            Quantity = 1,
            CustomerId = customer.Id,
            ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
            RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
            CustomerComments = model.Comments,
            StaffNotes = string.Empty,
            ReturnRequestStatus = ReturnRequestStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _returnRequestService.InsertReturnRequestAsync(rr);

        //set return request custom number
        rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
        await _customerService.UpdateCustomerAsync(customer);
        await _returnRequestService.UpdateReturnRequestAsync(rr);

        await _genericAttributeService.SaveAttributeAsync(rr, "RefundRequestInvoiceItemId", model.InvoiceItemId, rr.StoreId);

        //notify store owner
        await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
        //notify customer
        await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

        model = await _nexportPluginModelFactory.PrepareSubmitInvoiceItemRefundRequestModelAsync(model);
        model.Result = await _localizationService.GetResourceAsync("RedemptionUnassignmentRequests.Submitted");

        return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RequestRefund.cshtml", model);
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
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_EmailAddress", model.SelectedUserEmailAddress);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);
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
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_EmailAddress", model.Email);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_FirstName", model.FirstName);
                await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_LastName", model.LastName);
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
        var productId = await _genericAttributeService.GetAttributeAsync<int>(customer, "RedeemProductModel_productId");

        var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid>(customer, "RedeemProductModel_InvoiceItemId");

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
        //await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

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

            await _genericAttributeService.SaveAttributeAsync(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

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
                throw new Exception("Error retrieving invoice item id for transaction.");

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

    #endregion
}