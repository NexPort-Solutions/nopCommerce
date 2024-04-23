using System;
using FluentValidation;
using Nop.Web.Framework.Validators;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

namespace Nop.Plugin.Misc.Nexport.Validators
{
    public class RequestUnassignmentValidator : BaseNopValidator<SubmitRedemptionUnassignmentRequestModel>
    {
        public RequestUnassignmentValidator()
        {
            RuleFor(x=>x.Comments).NotEmpty();
        }
    }
}
