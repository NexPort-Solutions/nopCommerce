using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    [SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")]
    public class ConfigurationModel : BaseNopModel
    {
        [DataType(DataType.Url)]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Username")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Plugins.Misc.Nexport.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AutoRenewToken")]
        public bool AutoRenewToken { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.TokenNeverExpired")]
        public bool TokenNeverExpired { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.CustomTokenExpirationDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? CustomTokenExpirationDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Token")]
        public string NexportAuthenticationToken { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.UtcExpirationDate")]
        public DateTime? UtcExpirationDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RootOrganizationId")]
        public Guid? RootOrganizationId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.MerchantAccountId")]
        public Guid? MerchantAccountId { get; set; }
    }
}
