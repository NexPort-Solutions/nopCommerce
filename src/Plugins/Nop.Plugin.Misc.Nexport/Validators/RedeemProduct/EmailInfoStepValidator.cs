using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct;
using Nop.Web.Framework.Validators;
using NUglify.JavaScript.Syntax;


namespace Nop.Plugin.Misc.Nexport.Validators.RedeemProduct
{
    public class EmailInfoStepValidator : BaseNopValidator<EmailInfoStepModel>
    {
        public EmailInfoStepValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name cannot be empty");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name cannot be empty");
        }
    }
}
