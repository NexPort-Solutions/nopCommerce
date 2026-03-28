using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using IAuthenticationService = Nop.Services.Authentication.IAuthenticationService;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class NexportWholesaleActionFilter : IAsyncActionFilter
{
    private readonly IAuthenticationService _authenticationService;

    public NexportWholesaleActionFilter(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            await next();
            return;
        }

        if (actionDescriptor.ControllerTypeInfo != typeof(Controllers.NexportWholesaleController) ||
            !string.Equals(actionDescriptor.ActionName, nameof(Controllers.NexportWholesaleController.RedeemByEmail), StringComparison.Ordinal))
        {
            await next();
            return;
        }

        // Force the customer to re-authenticate when they click a direct redemption link
        var hasReferrer = !string.IsNullOrEmpty(context.HttpContext.Request.Headers.Referer.ToString());
        if (!hasReferrer)
        {
            await _authenticationService.SignOutAsync();
            context.Result = new ChallengeResult();
            return;
        }

        await next();
    }
}
