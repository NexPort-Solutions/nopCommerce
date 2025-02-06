using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class NexportReturnRequestController : BaseAdminController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly INotificationService _notificationService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IPermissionService _permissionService;
    private readonly IReturnRequestModelFactory _returnRequestModelFactory;
    private readonly IReturnRequestService _returnRequestService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly NexportService _nexportService;
    private readonly INexportWholesaleService _nexportWholesaleService;

    #endregion Fields

    #region Ctor

    public NexportReturnRequestController(ICustomerActivityService customerActivityService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IOrderService orderService,
        IProductService productService,
        IPermissionService permissionService,
        IReturnRequestModelFactory returnRequestModelFactory,
        IReturnRequestService returnRequestService,
        IWorkflowMessageService workflowMessageService,
        INexportPluginModelFactory nexportPluginModelFactory,
        NexportService nexportService,
        INexportWholesaleService nexportWholesaleService)
    {
        _customerActivityService = customerActivityService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _notificationService = notificationService;
        _orderService = orderService;
        _productService = productService;
        _permissionService = permissionService;
        _returnRequestModelFactory = returnRequestModelFactory;
        _returnRequestService = returnRequestService;
        _workflowMessageService = workflowMessageService;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _nexportService = nexportService;
        _nexportWholesaleService = nexportWholesaleService;
    }

    #endregion

    public virtual IActionResult Index()
    {
        return RedirectToAction("List", "NexportReturnRequest");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //prepare model
        var model =
            await _nexportPluginModelFactory.PrepareNexportReturnRequestSearchModelAsync(
                new NexportReturnRequestSearchModel());

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/List.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(NexportReturnRequestSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareNexportReturnRequestListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return RedirectToAction("List", "NexportReturnRequest");

        //prepare model
        var model = await _nexportPluginModelFactory.PrepareNexportReturnRequestModelAsync(null, returnRequest);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(NexportReturnRequestModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
        if (returnRequest == null)
            return RedirectToAction("List", "NexportReturnRequest");

        if (ModelState.IsValid)
        {
            var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid?>(returnRequest, "RefundRequestInvoiceItemId", returnRequest.StoreId);
            if (invoiceItemId != null)
            {
                var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId.Value);
                if (invoiceItem != null)
                {
                    // Refund the invoice item
                    await _nexportService.RefundInvoiceItem(invoiceItem);

                    // Drop the enrollment
                    if (invoiceItem.RedemptionEnrollmentId != null)
                    {
                        await _nexportService.DropEnrollmentAsync(invoiceItem.RedemptionEnrollmentId.Value)!;
                    }
                }
            }
            else
            {
                var quantityToReturn = model.Quantity;
                if (quantityToReturn > 0)
                {
                    var order = await _orderService.GetOrderByOrderItemAsync(returnRequest.OrderItemId);
                    if (order != null)
                    {
                        await _nexportService.RefundOrderItem(quantityToReturn, order.Id, returnRequest.OrderItemId);
                    }
                }
            }

            returnRequest = model.ToEntity(returnRequest);
            returnRequest.UpdatedOnUtc = DateTime.UtcNow;

            await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditReturnRequest",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditReturnRequest"),
                    returnRequest.Id), returnRequest);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.ReturnRequests.Updated"));

            return continueEditing
                ? RedirectToAction("Edit", "NexportReturnRequest", new { id = returnRequest.Id })
                : RedirectToAction("List", "NexportReturnRequest");
        }

        //prepare model
        model = await _nexportPluginModelFactory.PrepareNexportReturnRequestModelAsync(model, returnRequest, true);

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/Edit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return RedirectToAction("List");

        await _returnRequestService.DeleteReturnRequestAsync(returnRequest);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteReturnRequest",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteReturnRequest"), returnRequest.Id), returnRequest);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Deleted"));

        return RedirectToAction("List");
    }
}