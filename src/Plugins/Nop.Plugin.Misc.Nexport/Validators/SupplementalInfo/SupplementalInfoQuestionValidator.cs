using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Validators.SupplementalInfo;

public class SupplementalInfoQuestionValidator : BaseNopValidator<SupplementalInfoQuestionModel>
{
    public SupplementalInfoQuestionValidator(ILocalizationService _, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(supplementalInfoQuestionModel => supplementalInfoQuestionModel.QuestionText)
            .NotEmpty()
            .WithMessage("Question text is required");

        RuleFor(supplementalInfoQuestionModel => supplementalInfoQuestionModel.Description)
            .Length(0, 1000)
            .WithMessage("Description text cannot exceed 1000 characters");
        SetDatabaseValidationRules<Question>(mappingEntityAccessor);
    }
}
