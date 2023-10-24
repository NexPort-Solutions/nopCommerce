using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Validators;

public class SettingsValidator : BaseNopValidator<ConfigurationModel>
{
    public SettingsValidator()
    {
        RuleFor(configurationModel => configurationModel.Url).NotEmpty().WithMessage("Server url cannot be empty!");
        RuleFor(configurationModel => configurationModel.RootOrganizationId).NotEqual(Guid.Empty)
            .WithMessage("Root organization GroupGuid cannot be an empty GUID!");
        RuleFor(configurationModel => configurationModel.MerchantAccountId).NotEqual(Guid.Empty)
            .WithMessage("Merchant account GroupGuid cannot be an empty GUID!");
    }
}
