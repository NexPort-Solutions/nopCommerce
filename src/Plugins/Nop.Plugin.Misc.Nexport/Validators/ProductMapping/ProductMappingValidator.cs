using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Nexport.Validators.ProductMapping;

public class ProductMappingValidator : BaseNopValidator<ProductMappingModel>
{
    public ProductMappingValidator()
    {
        RuleFor(productMappingModel => productMappingModel.RenewalDuration)
            .NotEmpty()
            .When(productMappingModel => productMappingModel.AllowExtension)
            .WithMessage("Renewal duration is required when allowing the enrollment to be extended with new product purchase!");
        RuleFor(productMappingModel => productMappingModel.RenewalCompletionThreshold)
            .InclusiveBetween(0, 100)
            .When(productMappingModel => productMappingModel.AllowExtension && productMappingModel.RenewalCompletionThreshold is not null)
            .WithMessage("Renewal completion threshold value must between 0 and 100.");
        RuleFor(productMappingModel => productMappingModel.ExtensionPurchaseLimit)
            .GreaterThan(0)
            .When(productMappingModel => productMappingModel.AllowExtension && productMappingModel.ExtensionPurchaseLimit is not null)
            .WithMessage("Extension purchase limit value must be greater than 0.");
    }
}
