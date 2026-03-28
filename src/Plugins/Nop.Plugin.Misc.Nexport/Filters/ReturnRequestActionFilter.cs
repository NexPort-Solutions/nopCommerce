using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters;
public class ReturnRequestActionFilter(
    IGenericAttributeService genericAttributeService,
    ILocalizationService localizationService,
    IOrderService orderService,
    IReturnRequestService returnRequestService,
    NexportService nexportService)
    : ActionFilterAttribute
{
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            await next();
            return;
        }

        if (actionDescriptor.ControllerTypeInfo == typeof(ReturnRequestController) &&
            actionDescriptor.ActionName == "ReturnRequest" &&
            context.HttpContext.Request.Method == "POST")
        {
            //if (context.Result is ViewResult { Model: SubmitReturnRequestModel submitReturnRequestModel })
            //{
            //    if (submitReturnRequestModel.Result == await localizationService.GetResourceAsync("ReturnRequests.Submitted"))
            //    {
            //        var order = await orderService.GetOrderByIdAsync(submitReturnRequestModel.OrderId);
            //        if (order != null)
            //        {
            //            var orderItems = await orderService.GetOrderItemsAsync(order.Id);
            //            foreach (var item in orderItems)
            //            {
            //                var returnRequests = (await returnRequestService.SearchReturnRequestsAsync(order.StoreId, order.CustomerId, item.Id, rs: ReturnRequestStatus.Pending)).ToList();
            //                var nexportInvoiceItems = (await nexportService.FindNexportOrderInvoiceItems(order.Id, item.Id))
            //                    .Where(x => x.RedemptionStatus is
            //                        NexportOrderInvoiceItemRedemptionStatus.Available or
            //                        NexportOrderInvoiceItemRedemptionStatus.Awaiting or
            //                        NexportOrderInvoiceItemRedemptionStatus.Assigned).ToList();
            //                var wholesaleOrderInfo = await nexportService.GetWholesaleOrderInfoForOrderAsync(order.Id);
            //                if (wholesaleOrderInfo != null)
            //                {
            //                    if (returnRequests.Count == nexportInvoiceItems.Count)
            //                    {
            //                        for (var i = 0; i < nexportInvoiceItems.Count; i++)
            //                        {
            //                            var invoiceItem = nexportInvoiceItems[i];
            //                            var returnRequest = returnRequests[i];
            //                            switch (invoiceItem.RedemptionStatus)
            //                            {
            //                                case NexportOrderInvoiceItemRedemptionStatus.Assigned:
            //                                    wholesaleOrderInfo.Redeemed--;
            //                                    break;
            //                                case NexportOrderInvoiceItemRedemptionStatus.Available:
            //                                    wholesaleOrderInfo.Available--;
            //                                    break;
            //                                case NexportOrderInvoiceItemRedemptionStatus.Awaiting:
            //                                    wholesaleOrderInfo.Awaiting--;
            //                                    break;
            //                            }

            //                            wholesaleOrderInfo.ProcessingRefund++;
            //                            invoiceItem.RedemptionStatus = NexportOrderInvoiceItemRedemptionStatus.ProcessingRefund;

            //                            //await nexportService.UpdateNexportOrderInvoiceItem(invoiceItem);
            //                            //await nexportService.UpdateWholesaleOrderInfoAsync(wholesaleOrderInfo);

            //                            //await genericAttributeService.SaveAttributeAsync(returnRequest, "RefundRequestInvoiceItemId", invoiceItem.InvoiceItemId, order.StoreId);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        await base.OnResultExecutionAsync(context, next);
    }
}
