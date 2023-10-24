using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record LoginModel : Web.Models.Customer.LoginModel
{
    [NopResourceDisplayName("Account.Login.Fields.EmailOrUsername")]
    public string EmailOrUsername { get; set; }
}
