using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Data.Mapping;

namespace Nop.Plugin.Misc.Nexport.Validators.RegistrationField;

public class RegistrationFieldCategoryValidator : BaseNopValidator<CategoryModel>
{
    public RegistrationFieldCategoryValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(registrationFieldCategoryModel => registrationFieldCategoryModel.Title)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Nexport.RegistrationField.Category.Title.Required"));

        SetDatabaseValidationRules<Category>(mappingEntityAccessor);
    }
}
