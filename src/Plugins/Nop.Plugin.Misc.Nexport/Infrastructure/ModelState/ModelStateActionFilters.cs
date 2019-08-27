using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.ModelState
{
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }

    /// <summary>
    /// Model state exporting action filter
    /// </summary>
    public class ExportModelStateAttribute : ModelStateTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Export only when ModelState is not valid
            if (!filterContext.ModelState.IsValid)
            {
                // Export only for redirecting
                if (filterContext.Result is RedirectResult ||
                    filterContext.Result is RedirectToRouteResult ||
                    filterContext.Result is RedirectToActionResult)
                {
                    if (filterContext.Controller is Controller controller && filterContext.ModelState != null)
                    {
                        var modelState = ModelStateHelpers.SerializeModelState(filterContext.ModelState);
                        controller.TempData[Key] = modelState;
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    /// <summary>
    /// Model state importing action filter
    /// </summary>
    public class ImportModelStateAttribute : ModelStateTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as Controller;

            if (controller?.TempData[Key] is string serializedModelState)
            {
                // Import only for viewing
                if (filterContext.Result is ViewResult)
                {
                    var modelState = ModelStateHelpers.DeserializeModelState(serializedModelState);
                    filterContext.ModelState.Merge(modelState);
                }
                else
                {
                    // Otherwise remove it
                    controller.TempData.Remove(Key);
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
