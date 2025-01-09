using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Web.Controllers;
using Nop.Services.Authentication;

namespace Nop.Plugin.Misc.Nexport.Filters;

public class SignInActionFilter : ActionFilterAttribute
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IWorkContext _workContext;

    public SignInActionFilter(
        IAuthenticationService authenticationService,
        IWorkContext workContext)
    {
        _authenticationService = authenticationService;
        _workContext = workContext;
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        if (actionDescriptor.ActionName == nameof(CustomerController.Login) && HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (authenticatedUser != null)
            {
                if (string.IsNullOrWhiteSpace(authenticatedUser.FirstName) && string.IsNullOrWhiteSpace(authenticatedUser.LastName))
                {
                    var returnUrl = ((context.Result as RedirectResult)!).Url;
                    context.Result = new RedirectToActionResult("EditMissingCustomerInfo", "NexportIntegration", new { returnUrl = returnUrl });
                }
            }
        }

        await base.OnResultExecutionAsync(context, next);
    }
}