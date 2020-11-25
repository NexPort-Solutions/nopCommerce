using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldCategoryValidator : BaseNopValidator<NexportRegistrationFieldCategoryModel>
    {
        public NexportRegistrationFieldCategoryValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required"));

            SetDatabaseValidationRules<NexportRegistrationFieldCategory>(dataProvider);
        }
    }
}