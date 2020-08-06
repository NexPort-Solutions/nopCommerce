using FluentValidation;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldOptionValidator : BaseNopValidator<NexportRegistrationFieldOptionModel>
    {
        public NexportRegistrationFieldOptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.OptionValue)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Misc.Nexport.RegistrationField.Option.Value.Required"));

            var nexportPluginObjectContext = EngineContext.Current.Resolve<NexportPluginObjectContext>();
            SetDatabaseValidationRules<NexportRegistrationFieldOption>(nexportPluginObjectContext);
        }
    }
}
