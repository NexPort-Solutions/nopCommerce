using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct;
using Nop.Web.Framework.Validators;
using NUglify.JavaScript.Syntax;


namespace Nop.Plugin.Misc.Nexport.Validators.RedeemProduct
{
    public class CustomerStepValidator : BaseNopValidator<CustomerStepListSearchModel>
    {
        public CustomerStepValidator()
        {
            //RuleFor(x => x).Must(x => IsSomethingSelected(x.SendViaEmail, x.SelectedUserId)).WithMessage("You must either select a user or check redeem via email.").OverridePropertyName("EmailOrUserSelected");
            RuleFor(x => x.CustomerStepSendViaEmail)
                .Must((args, sendViaEmail) => IsSomethingSelected(sendViaEmail, args.SelectedUserId))
                .WithMessage("You must either select a user or check redeem via email.");
        }

        private bool IsSomethingSelected(bool sendViaEmail, Guid? selectedUserId)
        {
            if (sendViaEmail || (selectedUserId != null && selectedUserId != Guid.Empty))
            {
                return true;
            }

            return false;
        }
    }
}
