using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Validators.SupplementalInfo;

public class SupplementalInfoOptionValidator : BaseNopValidator<SupplementalInfoOptionModel>
{
    public SupplementalInfoOptionValidator(ILocalizationService _)
    {
        RuleFor(supplementalInfoOptionModel => supplementalInfoOptionModel.OptionText)
            .NotEmpty()
            .WithMessage("The text for question option is required");
    }
}
