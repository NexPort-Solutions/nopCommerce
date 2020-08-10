using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Validators.SupplementalInfo
{
    public class NexportSupplementalInfoOptionValidator : BaseNopValidator<NexportSupplementalInfoOptionModel>
    {
        public NexportSupplementalInfoOptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.OptionText)
                .NotEmpty()
                .WithMessage("The text for question option is required");
        }
    }
}
