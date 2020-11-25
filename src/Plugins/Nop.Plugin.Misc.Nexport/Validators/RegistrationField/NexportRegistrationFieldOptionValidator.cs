using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldOptionValidator : BaseNopValidator<NexportRegistrationFieldOptionModel>
    {
        public NexportRegistrationFieldOptionValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.OptionValue)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required"));

            SetDatabaseValidationRules<NexportRegistrationFieldOption>(dataProvider);
        }
    }
}
