using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Plugin.Misc.Nexport.Services;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class OrderDetailsActionFilter : ActionFilterAttribute
{
    private readonly ICustomerService _customer;
    private readonly IOrderService _order;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly INexportService _nexportService;
    private readonly IUserMappingService _userMapping;
    private readonly ISupplementalInfoService _supplementalInfo;

    public OrderDetailsActionFilter(
        ICustomerService customerService,
        IOrderService orderService,
        IStoreContext storeContext,
        IWorkContext workContext,
        INexportService nexportService,
        IUserMappingService userMapping,
        ISupplementalInfoService supplementalInfo)
    {
        _customer = customerService;
        _order = orderService;
        _storeContext = storeContext;
        _workContext = workContext;
        _nexportService = nexportService;
        _userMapping = userMapping;
        _supplementalInfo = supplementalInfo;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (actionDescriptor.ControllerTypeInfo == typeof(Web.Controllers.OrderController)
            && actionDescriptor.ActionName == nameof(Web.Controllers.OrderController.Details)
            && await _workContext.GetCurrentCustomerAsync() is { } customer
            && await _customer.IsRegisteredAsync(customer)
            && await _supplementalInfo.HasRequiredSupplementalInfo(customer.Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            && context.ActionArguments.TryGetValue("orderId", out var orderIdRaw)
            && orderIdRaw is int orderId and > 0
            && await _order.GetOrderByIdAsync(orderId) is { Deleted: false, CustomerId: var customerId }
            && customerId == customer.Id)
        {
            context.Result = new RedirectToActionResult(
                nameof(SupplementalInfoController.AnswerSupplementalInfoQuestion),
                ViewUtilities.GetControllerName<IntegrationController>(),
                new { returnResource = $"/orderdetails/{orderId}" });
        }
        await base.OnActionExecutionAsync(context, next);
    }
}
