using System.Linq;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class OrderDetailsActionFilter : ActionFilterAttribute
    {
        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly NexportService _nexportService;

        public OrderDetailsActionFilter(
            IOrderService orderService,
            IStoreContext storeContext,
            IWorkContext workContext,
            NexportService nexportService)
        {
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
                if (customer != null && customer.IsRegistered())
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
