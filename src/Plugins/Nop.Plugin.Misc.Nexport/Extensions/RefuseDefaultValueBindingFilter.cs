using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Extensions;

public class RefuseDefaultValueBindingFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.FirstOrDefault(BadApple) is { Key: var argumentName })
        {
            context.Result = new BadRequestObjectResult($"{argumentName} is required.");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
    private static object? GetDefaultValue(Type type) => Activator.CreateInstance(type);

    private static bool BadApple(KeyValuePair<string, object?> argument)
        => argument.Value?.GetType() is Type type
            && type.IsValueType
            && argument.Value.Equals(GetDefaultValue(type));
}
