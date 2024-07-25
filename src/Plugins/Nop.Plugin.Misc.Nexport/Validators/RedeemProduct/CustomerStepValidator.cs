using System;
using FluentValidation;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Web.Framework.Validators;


namespace Nop.Plugin.Misc.Nexport.Validators.RedeemProduct
{
    public class CustomerStepValidator : BaseNopValidator<CustomerStepModel>
    {
        public CustomerStepValidator()
        {

            RuleFor(x => x.Email)
                .NotEmpty()
                .Must(CommonHelper.IsValidEmail).WithMessage("Must be a valid email");
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.SelectedUserId).Must(SelectedOrNewUser).WithMessage("Must either select a customer or enter new customer email info.");
        }
        
        private bool SelectedOrNewUser(CustomerStepModel model, Guid? selectedUserId)
        {
            try
            {
                if(model.Email != null) 
                    return true;

                if (selectedUserId != null)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
