using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Services.Localization;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField;

public class RegistrationFieldValidator : BaseNopValidator<RegistrationFieldModel>
{
    public RegistrationFieldValidator(ILocalizationService localizationService)
    {
        RuleFor(registrationFieldModel => registrationFieldModel.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.Customers.CustomerAttributes.Fields.Name.Required"));

        RuleFor(registrationFieldModel => registrationFieldModel.CustomProfileFieldKey)
            .Length(3, 255)
            .WithMessage("The NexPort custom profile key must have length between 3 and 255 characters.")
            .Matches("^[a-zA-Z_]+[a-zA-Z0-9_]*$")
            .WithMessage("The NexPort custom profile key can only contain letters, numbers, and underscores, and must not start with a number.");
    }
}
