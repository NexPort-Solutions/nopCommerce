using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models;

public record ConfigurationModel : BaseNopModel
{
    [DataType(DataType.Url)]
    [NopResourceDisplayName("Plugins.Misc.Nexport.Url")]
    public required string Url { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Username")]
    public required string Username { get; set; }

    [DataType(DataType.Password)]
    [NopResourceDisplayName("Plugins.Misc.Nexport.Password")]
    public required string Password { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AutoRenewToken")]
    public required bool AutoRenewToken { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.TokenNeverExpired")]
    public required bool TokenNeverExpired { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.CustomTokenExpirationDate")]
    [UIHint("DateTimeNullable")]
    public required DateTime? CustomTokenExpirationDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Token")]
    public required string AuthenticationToken { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.UtcExpirationDate")]
    public required DateTime? UtcExpirationDate { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RootOrganizationId")]
    public required Guid RootOrganizationId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.MerchantAccountId")]
    public required Guid MerchantAccountId { get; set; }
}
