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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(OrderController) &&
                actionDescriptor.ActionName == nameof(OrderController.Details))
            {
                var customer = _workContext.CurrentCustomer;
                if (customer != null && _customerService.IsRegistered(customer))
                {
                    var store = _storeContext.CurrentStore;

                    var hasRequiredSupplementalInfo =
                        _nexportService.HasRequiredSupplementalInfo(customer.Id, store.Id);

                    if (hasRequiredSupplementalInfo)
                    {
                        context.ActionArguments.TryGetValue("orderId", out var orderIdValue);

                        if (orderIdValue is int orderId && orderId > 0)
                        {
                            var order = _orderService.GetOrderById(orderId);

                            if (order != null && !order.Deleted && order.CustomerId == customer.Id)
                            {
                                context.Result = new RedirectToActionResult("AnswerSupplementalInfoQuestion",
                                    "NexportIntegration", new { returnUrl = $"/orderdetails/{orderId}"});
                            }
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
