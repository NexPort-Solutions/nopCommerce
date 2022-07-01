using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public record NexportLoginModel : LoginModel
    {
        [NopResourceDisplayName("Account.Login.Fields.EmailOrUsername")]
        public string EmailOrUsername { get; set; }
    }
}