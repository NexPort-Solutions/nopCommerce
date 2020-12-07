using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Nexport.Validators.ProductMapping
{
    public class NexportProductMappingValidator : BaseNopValidator<NexportProductMappingModel>
    {
        public NexportProductMappingValidator()
        {
            RuleFor(x => x.RenewalDuration)
                .NotEmpty()
                .When(x => x.AllowExtension)
                .WithMessage("Renewal duration is required when allowing the enrollment to be extended with new product purchase!");
            RuleFor(x => x.RenewalCompletionThreshold)
                .InclusiveBetween(0, 100)
                .When(x => x.AllowExtension && x.RenewalCompletionThreshold.HasValue)
                .WithMessage("Renewal completion threshold value must between 0 and 100.");
            RuleFor(x => x.ExtensionPurchaseLimit)
                .GreaterThan(0)
                .When(x => x.AllowExtension && x.ExtensionPurchaseLimit.HasValue)
                .WithMessage("Extension purchase limit value must be greater than 0.");
        }
    }
}
