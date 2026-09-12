using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Nexport.Validators.Customer;

public class NexportRegisterValidator : BaseNopValidator<NexportRegisterModel>
{
    public NexportRegisterValidator(IValidator<RegisterModel> registerValidator)
    {
        Include(registerValidator);
    }
}
