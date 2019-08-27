using System;
using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Validators
{
    public class NexportSettingsValidator : BaseNopValidator<ConfigurationModel>
    {
        public NexportSettingsValidator()
        {
            RuleFor(x => x.Url).NotEmpty().WithMessage("Server url cannot be empty!");
            RuleFor(x => x.RootOrganizationId).NotEqual(Guid.Empty)
                .WithMessage("Root organization Id cannot be an empty GUID!");
            RuleFor(x => x.MerchantAccountId).NotEqual(Guid.Empty)
                .WithMessage("Merchant account Id cannot be an empty GUID!");
        }
    }
}
