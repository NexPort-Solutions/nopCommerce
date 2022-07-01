using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Data.Mapping;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldOptionValidator : BaseNopValidator<NexportRegistrationFieldOptionModel>
    {
        public NexportRegistrationFieldOptionValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.OptionValue)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required"));

            SetDatabaseValidationRules<NexportRegistrationFieldOption>(mappingEntityAccessor);
        }
    }
}
