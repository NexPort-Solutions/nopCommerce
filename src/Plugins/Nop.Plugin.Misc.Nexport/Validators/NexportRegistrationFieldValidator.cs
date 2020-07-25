using FluentValidation;
using Nop.Data;
using Nop.Web.Framework.Validators;
using Nop.Services.Localization;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Validators
{
    public class NexportRegistrationFieldValidator : BaseNopValidator<NexportRegistrationFieldModel>
    {
        public NexportRegistrationFieldValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Customers.CustomerAttributes.Fields.Name.Required"));

            RuleFor(x => x.NexportCustomProfileFieldKey)
                .Length(3, 255)
                .WithMessage("The Nexport custom profile key must have length between 3 and 255 characters.")
                .Matches("^[a-zA-Z_]+[a-zA-Z0-9_]*$")
                .WithMessage("The Nexport custom profile key can only contain letters, numbers, and underscores, and must not start with a number.");
        }
    }
}
