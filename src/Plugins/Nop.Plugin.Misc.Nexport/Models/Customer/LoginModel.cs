using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public partial class NexportLoginModel : BaseNopModel
    {
        public bool CheckoutAsGuest { get; set; }

        //[DataType(DataType.Custom)]
        [NopResourceDisplayName("Account.Login.Fields.EmailOrUsername")]
        public string EmailOrUsername { get; set; }

        public bool UsernamesEnabled { get; set; }

        public UserRegistrationType RegistrationType { get; set; }

        [DataType(DataType.Password)]
        [NoTrim]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}