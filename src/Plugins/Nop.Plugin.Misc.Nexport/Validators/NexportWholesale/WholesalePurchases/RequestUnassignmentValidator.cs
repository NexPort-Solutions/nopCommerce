using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Nexport.Validators.NexportWholesale.WholesalePurchases;

public class RequestUnassignmentValidator : BaseNopValidator<SubmitRedemptionUnassignmentRequestModel>
{
    public RequestUnassignmentValidator()
    {
        RuleFor(x => x.Comments).NotEmpty();
    }
}