using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
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

public class NexportReturnRequestController(
    ICustomerActivityService customerActivityService,
    ILocalizationService localizationService,
    ILocalizedEntityService localizedEntityService,
    INotificationService notificationService,
    IOrderService orderService,
    IProductService productService,
    IPermissionService permissionService,
    IReturnRequestModelFactory returnRequestModelFactory,
    IReturnRequestService returnRequestService,
    IWorkflowMessageService workflowMessageService,
    IGenericAttributeService genericAttributeService,
    IWorkContext workContext,
    INexportPluginModelFactory nexportPluginModelFactory,
    NexportService nexportService,
    INexportWholesaleService nexportWholesaleService)
    : ReturnRequestController(
        customerActivityService,
        localizationService,
        localizedEntityService,
        notificationService,
        orderService,
        productService,
        permissionService,
        returnRequestModelFactory,
        returnRequestService,
        workflowMessageService)
{
    public override IActionResult Index()
    {
        return RedirectToAction("List", "NexportReturnRequest");
    }

    [Route("Admin/ReturnRequest/List")]
    public override async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //prepare model
        var model = await nexportPluginModelFactory.PrepareNexportReturnRequestSearchModelAsync(new NexportReturnRequestSearchModel());

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/List.cshtml", model);
    }

    [HttpPost]
    [Route("Admin/ReturnRequest/List")]
    public virtual async Task<IActionResult> List(NexportReturnRequestSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await nexportPluginModelFactory.PrepareNexportReturnRequestListModelAsync(searchModel);

        return Json(model);
    }

    [Route("Admin/ReturnRequest/Nexport/Edit/{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return RedirectToAction("List", "NexportReturnRequest");

        //prepare model
        var model = await nexportPluginModelFactory.PrepareNexportReturnRequestModelAsync(null, returnRequest);

        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/Edit.NexportReturn.cshtml", model);
    }

    [Route("Admin/ReturnRequest/Nexport/Edit/{id}")]
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
            if (model.ReturnRequestStatusId == (int)ReturnRequestStatus.ItemsRefunded)
            {

                var invoiceItemId = await genericAttributeService.GetAttributeAsync<Guid?>(returnRequest, "RefundRequestInvoiceItemId", returnRequest.StoreId);
                if (invoiceItemId != null)
                {
                    var invoiceItem = await nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId.Value);
                    if (invoiceItem != null)
                    {
                        switch ((NexportRefundOptionEnums)model.RefundOption)
                        {
                            default:
                            case NexportRefundOptionEnums.ExpireEnrollment:
                                if (invoiceItem.RedemptionEnrollmentId != null)
                                {
                                    await nexportService.ResetInvoiceRedemptionAsync(invoiceItem, $"Expiring enrollment {invoiceItem.RedemptionEnrollmentId} due to refund!");
                                }
                                break;

                            case NexportRefundOptionEnums.DropEnrollment:
                                if (invoiceItem.RedemptionEnrollmentId != null)
                                {
                                    await nexportService.DropEnrollmentAsync(invoiceItem.RedemptionEnrollmentId.Value)!;
                                }
                                break;

                            case NexportRefundOptionEnums.DestroyEnrollment:
                                if (invoiceItem.RedemptionEnrollmentId != null)
                                {
                                    await nexportService.DestroyEnrollmentAsync(invoiceItem.RedemptionEnrollmentId.Value)!;
                                }
                                break;
                        }

                        // Refund the invoice item
                        await nexportService.RefundInvoiceItem(invoiceItem);
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
                            await nexportService.RefundOrderItem(quantityToReturn, order.Id, returnRequest.OrderItemId);
                        }
                    }
                }
            }

            returnRequest = model.ToEntity(returnRequest);
            returnRequest.UpdatedOnUtc = DateTime.UtcNow;

            await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

            var currentCustomer = await workContext.GetCurrentCustomerAsync();
            await genericAttributeService.SaveAttributeAsync(returnRequest, "ModifiedByUser", currentCustomer.Id, returnRequest.StoreId);

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
        model = await nexportPluginModelFactory.PrepareNexportReturnRequestModelAsync(model, returnRequest, true);

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/Edit.NexportReturn.cshtml", model);
    }
}