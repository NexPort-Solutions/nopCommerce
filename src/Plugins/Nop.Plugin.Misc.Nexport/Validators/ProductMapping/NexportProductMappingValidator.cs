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
        }
    }
}
