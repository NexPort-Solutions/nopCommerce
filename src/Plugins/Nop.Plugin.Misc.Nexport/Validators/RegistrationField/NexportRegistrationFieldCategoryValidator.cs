using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Data.Mapping;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField
{
    public class NexportRegistrationFieldCategoryValidator : BaseNopValidator<NexportRegistrationFieldCategoryModel>
    {
        public NexportRegistrationFieldCategoryValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required"));

            SetDatabaseValidationRules<NexportRegistrationFieldCategory>(mappingEntityAccessor);
        }
    }
}