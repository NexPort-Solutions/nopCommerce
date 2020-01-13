using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ProductEditActionFilter : ActionFilterAttribute
    {
        private readonly INotificationService _notificationService;
        private readonly IProductService _productService;
        private readonly NexportService _nexportService;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public ProductEditActionFilter(
            INotificationService notificationService,
            IProductService productService,
            NexportService nexportService,
            ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _notificationService = notificationService;
            _productService = productService;
            _nexportService = nexportService;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == "Edit" &&
                context.HttpContext.Request.Method == "GET")
            {
                var tempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext);
                var messages = tempData.ContainsKey(NopMessageDefaults.NotificationListKey)
                    ? JsonConvert.DeserializeObject<IList<NotifyData>>(tempData[NopMessageDefaults.NotificationListKey].ToString())
                    : new List<NotifyData>();

                var hasMappingError = messages.Any(x => x.Message.Contains("default mapping with Nexport product"));
                if (!hasMappingError)
                {
                    if (context.Result is ViewResult result && result.Model is ProductModel productModel)
                    {
                        var hasAnyNexportMappings = _nexportService.HasNexportProductMapping(productModel.Id);
                        var hasDefaultMapping = _nexportService.HasDefaultMapping(productModel.Id);

                        if (hasAnyNexportMappings && !hasDefaultMapping)
                        {
                            _notificationService.WarningNotification(
                                "This product is missing a default mapping with Nexport product!");
                        }
                    }
                }
            }

            base.OnResultExecuting(context);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == "Edit" &&
                context.HttpContext.Request.Method == "POST")
            {
                var productId = GetProductId(context);

                context.ActionArguments.TryGetValue("continueEditing", out var continueEditingValue);

                var continueEditing = false;
                if (continueEditingValue != null)
                    continueEditing = (bool)continueEditingValue;

                if (productId != null)
                {
                    var hasAnyNexportMappings = _nexportService.HasNexportProductMapping(productId.Value);
                    var hasDefaultMapping = _nexportService.HasDefaultMapping(productId.Value);

                    if (hasAnyNexportMappings && !hasDefaultMapping)
                    {
                        _notificationService.ErrorNotification(
                            "This product does not have a default mapping with Nexport product. Please map it before selecting store displaying.");

                        ((Controller)(context.Controller)).ViewBag.MappingError = true;
                        context.Result = continueEditing ?
                            new RedirectToActionResult("Edit", "Product", productId.Value) :
                            new RedirectToActionResult("List", "Product", null);
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        private static int? GetProductId(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("model", out var value)
                && value is ProductModel model)
            {
                return model.Id;
            }

            return null;
        }
    }
}
