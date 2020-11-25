using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Validators.SupplementalInfo
{
    public class NexportSupplementalInfoQuestionValidator : BaseNopValidator<NexportSupplementalInfoQuestionModel>
    {
        public NexportSupplementalInfoQuestionValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.QuestionText)
                .NotEmpty()
                .WithMessage("Question text is required");

            RuleFor(x => x.Description)
                .Length(0, 1000)
                .WithMessage("Description text cannot exceed 1000 characters");

            SetDatabaseValidationRules<NexportSupplementalInfoQuestion>(dataProvider);
        }
    }
}