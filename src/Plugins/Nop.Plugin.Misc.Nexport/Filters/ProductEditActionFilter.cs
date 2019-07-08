using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Filters
{
    public class ProductEditActionFilter : ActionFilterAttribute
    {
        private readonly IProductService _productService;
        private readonly NexportService _nexportService;

        public ProductEditActionFilter(
            IProductService productService,
            NexportService nexportService)
        {
            _productService = productService;
            _nexportService = nexportService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
                return;

            int? productId = null;
            ProductModel productBeforeModified = null;

            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == "Edit" &&
                context.HttpContext.Request.Method == "POST")
            {
                productId = GetProductId(context);
                if (productId != null)
                {
                    productBeforeModified = _productService.GetProductById(productId.Value).ToModel<ProductModel>();
                }
            }

            await next();

            if (actionDescriptor.ControllerTypeInfo == typeof(ProductController) &&
                actionDescriptor.ActionName == "Edit" &&
                context.HttpContext.Request.Method == "POST")
            {
                if (productId != null)
                {
                    var nexportMapping = _nexportService.GetProductMappingByNopProductId(productId.Value);
                    if (nexportMapping != null && nexportMapping.IsSynchronized)
                    {
                        var currentProduct = _productService.GetProductById(productId.Value);
                        if (productBeforeModified.Name != currentProduct.Name ||
                            productBeforeModified.FullDescription != currentProduct.FullDescription ||
                            productBeforeModified.ShortDescription != currentProduct.ShortDescription && nexportMapping.Type == NexportProductTypeEnum.Section ||
                            productBeforeModified.Sku != currentProduct.Sku && nexportMapping.Type == NexportProductTypeEnum.Section ||
                            productBeforeModified.AvailableStartDateTimeUtc != currentProduct.AvailableStartDateTimeUtc && (nexportMapping.Type == NexportProductTypeEnum.Section || nexportMapping.Type == NexportProductTypeEnum.TrainingPlan) ||
                            productBeforeModified.AvailableEndDateTimeUtc != currentProduct.AvailableEndDateTimeUtc && (nexportMapping.Type == NexportProductTypeEnum.Section || nexportMapping.Type == NexportProductTypeEnum.TrainingPlan))
                        {
                            nexportMapping.IsSynchronized = false;
                            nexportMapping.UtcLastSynchronizationDate = null;

                            _nexportService.UpdateMapping(nexportMapping);
                        }
                    }
                }
            }
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
