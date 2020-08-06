using FluentValidation;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldCategoryValidator : BaseNopValidator<NexportRegistrationFieldCategoryModel>
    {
        public NexportRegistrationFieldCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required"));

            var nexportPluginObjectContext = EngineContext.Current.Resolve<NexportPluginObjectContext>();
            SetDatabaseValidationRules<NexportRegistrationFieldCategory>(nexportPluginObjectContext);
        }
    }
}