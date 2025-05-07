using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    public new virtual async Task<IActionResult> Edit(int id)
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
    [HttpPost, ParameterBasedOnFormName("deny", "denyRefund")]
    [FormValueRequired("accept", "deny")]
    public virtual async Task<IActionResult> Edit(NexportReturnRequestModel model, bool denyRefund)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
        if (returnRequest == null)
            return RedirectToAction("List", "NexportReturnRequest");

        if (ModelState.IsValid)
        {
            if (model.ReturnRequestStatusId == (int)ReturnRequestStatus.Pending)
            {
                try
                {
                    var currentCustomer = await workContext.GetCurrentCustomerAsync();

                    if (model.Quantity == 1)
                    {
                        var invoiceItemIds = await genericAttributeService.GetAttributeAsync<string>(returnRequest, "RefundRequestInvoiceItems", returnRequest.StoreId);
                        if (!string.IsNullOrWhiteSpace(invoiceItemIds))
                        {
                            var invoiceItemList = JsonConvert.DeserializeObject<List<Guid>>(invoiceItemIds);
                            if (invoiceItemList.Count == 1)
                            {
                                var invoiceItemId = invoiceItemList.First();
                                var invoiceItem =
                                    await nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);
                                if (invoiceItem != null)
                                {
                                    if (denyRefund)
                                    {
                                        // Deny the refund request
                                        await nexportService.ProcessRefundingInvoiceItem(invoiceItem, false);

                                        returnRequest.ReturnRequestStatus = ReturnRequestStatus.RequestRejected;
                                    }
                                    else
                                    {
                                        switch ((NexportRefundOptionEnums)model.RefundOption)
                                        {
                                            default:
                                            case NexportRefundOptionEnums.ExpireEnrollment:
                                                if (invoiceItem.RedemptionEnrollmentId != null)
                                                {
                                                    await nexportService.ResetInvoiceRedemptionAsync(invoiceItem,
                                                        $"Expiring enrollment {invoiceItem.RedemptionEnrollmentId} due to refund!");
                                                }

                                                break;

                                            case NexportRefundOptionEnums.DropEnrollment:
                                                if (invoiceItem.RedemptionEnrollmentId != null)
                                                {
                                                    await nexportService.DropEnrollmentAsync(invoiceItem
                                                        .RedemptionEnrollmentId.Value)!;
                                                }

                                                break;

                                            case NexportRefundOptionEnums.DestroyEnrollment:
                                                if (invoiceItem.RedemptionEnrollmentId != null)
                                                {
                                                    await nexportService.DestroyEnrollmentAsync(invoiceItem
                                                        .RedemptionEnrollmentId.Value)!;
                                                }

                                                break;
                                        }

                                        // Refund the invoice item
                                        await nexportService.ProcessRefundingInvoiceItem(invoiceItem, true);

                                        returnRequest.ReturnRequestStatus = ReturnRequestStatus.ItemsRefunded;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var order = await _orderService.GetOrderByOrderItemAsync(returnRequest.OrderItemId);
                        if (order != null)
                        {
                            await nexportService.RefundOrderItem(model.Quantity, order.Id, returnRequest.OrderItemId);
                            returnRequest.ReturnRequestStatus = ReturnRequestStatus.ItemsRefunded;
                        }
                    }

                    returnRequest = model.ToEntity(returnRequest);
                    returnRequest.UpdatedOnUtc = DateTime.UtcNow;

                    await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

                    await genericAttributeService.SaveAttributeAsync(returnRequest, "ModifiedByUser", currentCustomer.Id, returnRequest.StoreId);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("EditReturnRequest",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditReturnRequest"),
                            returnRequest.Id), returnRequest);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Updated"));

                    return RedirectToAction("Edit", "NexportReturnRequest", new { id = returnRequest.Id });
                }
                catch (Exception ex)
                {
                    _notificationService.ErrorNotification("Unable to process request refund invoice item!");
                }
            }
        }

        //prepare model
        model = await nexportPluginModelFactory.PrepareNexportReturnRequestModelAsync(model, returnRequest, true);

        //if we got this far, something failed, redisplay form
        return View("~/Plugins/Misc.Nexport/Areas/Admin/Views/ReturnRequest/Edit.NexportReturn.cshtml", model);
    }

    [Route("Admin/ReturnRequest/Nexport/Delete/{id}")]
    [HttpPost]
    public new virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AccessDeniedView();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return RedirectToAction("List");

        if (returnRequest.ReturnRequestStatusId == (int)ReturnRequestStatus.Pending)
        {
            if (returnRequest.Quantity == 1)
            {
                var invoiceItemIds = await genericAttributeService.GetAttributeAsync<string>(returnRequest, "RefundRequestInvoiceItems", returnRequest.StoreId);
                if (!string.IsNullOrWhiteSpace(invoiceItemIds))
                {
                    var invoiceItemList = JsonConvert.DeserializeObject<List<Guid>>(invoiceItemIds);
                    if (invoiceItemList.Count == 1)
                    {
                        var invoiceItemId = invoiceItemList.First();
                        var invoiceItem = await nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);
                        if (invoiceItem != null)
                        {
                            // Deny the refund request
                            await nexportService.ProcessRefundingInvoiceItem(invoiceItem, false);
                        }
                    }
                }
            }
        }

        await _returnRequestService.DeleteReturnRequestAsync(returnRequest);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteReturnRequest",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteReturnRequest"), returnRequest.Id), returnRequest);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Deleted"));

        return RedirectToAction("List");
    }
}