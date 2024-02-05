using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct;
using Nop.Web.Framework.Validators;
using NUglify.JavaScript.Syntax;


namespace Nop.Plugin.Misc.Nexport.Validators.RedeemProduct
{
    public class EmailStepValidator : BaseNopValidator<EmailStepModel>
    {
        public EmailStepValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email cannot be empty");
            RuleFor(x=>x.Email).Must(IsValidEmail).WithMessage("Must be a valid email");
        }

        private bool IsValidEmail(string? email)
        {
            if (email == null)  
                return false;
            try {
                
                return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            }
            catch {
                return false;
            }
        }
    }
}
