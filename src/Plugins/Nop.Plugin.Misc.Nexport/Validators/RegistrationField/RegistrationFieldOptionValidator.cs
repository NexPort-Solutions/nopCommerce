using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Data.Mapping;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField;

public class RegistrationFieldOptionValidator : BaseNopValidator<OptionModel>
{
    public RegistrationFieldOptionValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(registrationFieldOptionModel => registrationFieldOptionModel.OptionValue)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required"));

        SetDatabaseValidationRules<Option>(mappingEntityAccessor);
    }
}
