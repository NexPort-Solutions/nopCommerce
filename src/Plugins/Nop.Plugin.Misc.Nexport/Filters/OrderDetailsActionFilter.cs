using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class OrderDetailsActionFilter : ActionFilterAttribute
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly NexportService _nexportService;

        public OrderDetailsActionFilter(
            ICustomerService customerService,
            IOrderService orderService,
            IStoreContext storeContext,
            IWorkContext workContext,
            NexportService nexportService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _storeContext = storeContext;
            _workContext = workContext;
            _nexportService = nexportService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(OrderController) &&
                actionDescriptor.ActionName == nameof(OrderController.Details))
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer != null && await _customerService.IsRegisteredAsync(customer))
                {
                    var store = await _storeContext.GetCurrentStoreAsync();

                    var hasRequiredSupplementalInfo =
                        await _nexportService.HasRequiredSupplementalInfo(customer.Id, store.Id);

                    if (hasRequiredSupplementalInfo)
                    {
                        context.ActionArguments.TryGetValue("orderId", out var orderIdValue);

                        if (orderIdValue is int orderId and > 0)
                        {
                            var order = await _orderService.GetOrderByIdAsync(orderId);

                            if (order is { Deleted: false } && order.CustomerId == customer.Id)
                            {
                                context.Result = new RedirectToActionResult("AnswerSupplementalInfoQuestion",
                                    "NexportIntegration", new { returnUrl = $"/orderdetails/{orderId}" });
                            }
                        }
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
