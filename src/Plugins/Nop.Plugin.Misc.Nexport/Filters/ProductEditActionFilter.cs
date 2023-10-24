using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Services;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class ProductEditActionFilter : ActionFilterAttribute
{
    private readonly INotificationService _notification;
    private readonly IProductService _product;
    private readonly INexportService _nexportService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IProductMappingService _productMappingService;

    public ProductEditActionFilter(
        INotificationService notificationService,
        IProductService productService,
        INexportService nexportService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IProductMappingService productMappingService)
    {
        _notification = notificationService;
        _product = productService;
        _nexportService = nexportService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _productMappingService = productMappingService;
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (ActionIsProductEdit(actionDescriptor)
            && context.HttpContext.Request.Method == HttpMethods.Get
            && mappingSuccessful(context)
            && context.Result is ViewResult { Model: ProductModel { Id: var id } }
            && await ProductIsManuallyMapped(id))
        {
            _notification.WarningNotification("This product is missing a default mapping with NexPort product!");
        }
        await base.OnResultExecutionAsync(context, next);

        bool mappingSuccessful(ResultExecutingContext context)
            => !(_tempDataDictionaryFactory.GetTempData(context.HttpContext).TryGetValue(NopMessageDefaults.NotificationListKey, out var listKey)
                && listKey?.ToString() is string key
                && JsonConvert.DeserializeObject<IList<NotifyData>>(key) is { } data
                && data.Any(data => data.Message.Contains("default mapping with NexPort product", StringComparison.OrdinalIgnoreCase)));
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }
        if (ActionIsProductEdit(actionDescriptor)
            && context.HttpContext.Request.Method == HttpMethods.Post
            && GetProductId(context) is { } productId
            && await ProductIsManuallyMapped(productId)
            && context.Controller is Controller controller)
        {
            _notification.ErrorNotification("This product does not have a default mapping with NexPort product. Please map it before selecting store displaying.");
            controller.ViewBag.MappingError = true;
            context.Result = context.ActionArguments.TryGetValue("continueEditing", out var continueEditingValue) && continueEditingValue is true
                ? new RedirectToActionResult(nameof(ProductController.Edit), ViewUtilities.GetControllerName<ProductController>(), productId)
                : new RedirectToActionResult(nameof(ProductController.List), ViewUtilities.GetControllerName<ProductController>(), null);
        }
        await base.OnActionExecutionAsync(context, next);
    }

    private bool ActionIsProductEdit(ControllerActionDescriptor actionDescriptor)
        => actionDescriptor.ControllerTypeInfo == typeof(ProductController) && actionDescriptor.ActionName == nameof(ProductController.Edit);

    private async ValueTask<bool> ProductIsManuallyMapped(int productId)
        => await _productMappingService.HasProductMapping(productId) && !await _productMappingService.HasDefaultMapping(productId);

    private static int? GetProductId(ActionExecutingContext context)
        => context.ActionArguments.TryGetValue("model", out var value) && value is ProductModel model ? model.Id : null;
}
