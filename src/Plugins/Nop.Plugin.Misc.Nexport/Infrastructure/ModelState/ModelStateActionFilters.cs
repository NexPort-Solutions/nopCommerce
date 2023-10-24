using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState;

public abstract class ModelStateTransfer : ActionFilterAttribute
{
    protected const string KEY = nameof(ModelStateTransfer);
}

/// <summary>
/// Model state exporting action filter
/// </summary>
public sealed class ExportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Export only when ModelState is not valid and for redirecting
        if (!context.ModelState.IsValid
            && context.Result is RedirectResult or RedirectToRouteResult or RedirectToActionResult
            && context.Controller is Controller controller
            && context.ModelState is { } modelState)
        {
            controller.TempData[KEY] = ModelStateHelpers.SerializeModelState(modelState);
        }
        base.OnActionExecuted(context);
    }
}

/// <summary>
/// Model state importing action filter
/// </summary>
public sealed class ImportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is not Controller controller)
        {
            base.OnActionExecuted(context);
            return;
        }
        if (context.Result is not ViewResult)
        {
            // Remove it if not viewing
            controller.TempData.Remove(KEY);
        }
        else if (controller.TempData[KEY] is string serializedModelState)
        {
            var modelState = ModelStateHelpers.DeserializeModelState(serializedModelState);
            context.ModelState.Merge(modelState);
        }
        base.OnActionExecuted(context);
    }
}
