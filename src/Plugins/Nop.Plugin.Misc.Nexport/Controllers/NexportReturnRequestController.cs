using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public class NexportReturnRequestController(
    ICustomerService customerService,
    ICustomNumberFormatter customNumberFormatter,
    IDownloadService downloadService,
    ILocalizationService localizationService,
    INopFileProvider fileProvider,
    IOrderProcessingService orderProcessingService,
    IOrderService orderService,
    IReturnRequestModelFactory returnRequestModelFactory,
    IReturnRequestService returnRequestService,
    IStoreContext storeContext,
    IWorkContext workContext,
    IWorkflowMessageService workflowMessageService,
    LocalizationSettings localizationSettings,
    OrderSettings orderSettings,
    IGenericAttributeService genericAttributeService,
    NexportService nexportService)
    : ReturnRequestController(
        customerService,
        customNumberFormatter,
        downloadService,
        localizationService,
        fileProvider,
        orderProcessingService,
        orderService,
        returnRequestModelFactory,
        returnRequestService,
        storeContext,
        workContext,
        workflowMessageService,
        localizationSettings,
        orderSettings)
{
    [Route("ReturnRequest/{orderId:int}")]
    public virtual async Task<IActionResult> ReturnRequest(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
            return RedirectToRoute("Homepage");

        var model = new SubmitReturnRequestModel();
        model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
        return View("~/Plugins/Misc.Nexport/Views/ReturnRequest/ReturnRequest.cshtml", model);
    }

    [Route("ReturnRequest/{orderId:int}")]
    [HttpPost, ActionName("ReturnRequest")]
    public override async Task<IActionResult> ReturnRequestSubmit(int orderId, SubmitReturnRequestModel model, IFormCollection form)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();

        if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
            return RedirectToRoute("Homepage");

        var count = 0;

        var downloadId = 0;
        if (_orderSettings.ReturnRequestsAllowFiles)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(model.UploadedFileGuid);
            if (download != null)
                downloadId = download.Id;
        }

        //returnable products
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);
        foreach (var orderItem in orderItems)
        {
            var quantity = 0; //parse quantity
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"quantity{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(form[formKey], out quantity);
                    break;
                }
            if (quantity > 0)
            {
                var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.ReturnRequestReasonId);
                var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(model.ReturnRequestActionId);
                var store = await _storeContext.GetCurrentStoreAsync();

                var rr = new ReturnRequest
                {
                    CustomNumber = "",
                    StoreId = store.Id,
                    OrderItemId = orderItem.Id,
                    Quantity = quantity,
                    CustomerId = customer.Id,
                    ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                    RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                    CustomerComments = model.Comments,
                    UploadedFileId = downloadId,
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

                var invoiceItems = (await nexportService.FindNexportOrderInvoiceItems(order.Id, orderItem.Id))
                    .Where(x=> x.RedemptionStatus is
                        NexportOrderInvoiceItemRedemptionStatus.Available or
                        NexportOrderInvoiceItemRedemptionStatus.Awaiting or
                        NexportOrderInvoiceItemRedemptionStatus.Assigned);

                var nexportOrderInvoiceItems = invoiceItems.ToList();
                if (nexportOrderInvoiceItems.Count == quantity)
                {
                    var wholesaleOrderInfo = await nexportService.GetWholesaleOrderInfoForOrderAsync(order.Id);

                    foreach (var nexportOrderInvoiceItem in nexportOrderInvoiceItems)
                    {
                        switch (nexportOrderInvoiceItem.RedemptionStatus)
                        {
                            case NexportOrderInvoiceItemRedemptionStatus.Assigned:
                                wholesaleOrderInfo.Redeemed--;
                                break;
                            case NexportOrderInvoiceItemRedemptionStatus.Available:
                                wholesaleOrderInfo.Available--;
                                break;
                            case NexportOrderInvoiceItemRedemptionStatus.Awaiting:
                                wholesaleOrderInfo.Awaiting--;
                                break;
                        }
                        nexportOrderInvoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund;
                        await nexportService.UpdateNexportOrderInvoiceItem(nexportOrderInvoiceItem);
                    }

                    wholesaleOrderInfo.ProcessingRefund+= quantity;
                    await nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

                    await genericAttributeService.SaveAttributeAsync(rr, "RefundRequestInvoiceItems",
                        JsonConvert.SerializeObject(nexportOrderInvoiceItems.Select(x => x.InvoiceItemId).ToList()), order.StoreId);
                }

                //notify store owner
                await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                count++;
            }
        }

        model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
        if (count > 0)
            model.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");
        else
            model.Result = await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted");

        return View("~/Plugins/Misc.Nexport/Views/ReturnRequest/ReturnRequest.cshtml", model);
    }
}
