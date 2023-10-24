using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Nexport;

public class Settings : ISettings
{
    [DataType(DataType.Url)]
    public string? Url { get; set; }

    public string? AuthenticationToken { get; set; }
    public DateTime? UtcExpirationDate { get; set; }
    public Guid? RootOrganizationId { get; set; }
    public Guid? MerchantAccountId { get; set; }
    public bool IgnoreAcl { get; set; }
    public bool IgnoreStoreLimitations { get; set; }

    [MemberNotNullWhen(true, nameof(Url))]
    [MemberNotNullWhen(true, nameof(AuthenticationToken))]
    [MemberNotNullWhen(true, nameof(UtcExpirationDate))]
    [MemberNotNullWhen(true, nameof(RootOrganizationId))]
    [MemberNotNullWhen(true, nameof(MerchantAccountId))]
    public bool IsValid()
    {
        return Url is not null
            && AuthenticationToken is not null
            && UtcExpirationDate is not null
            && RootOrganizationId is not null
            && MerchantAccountId is not null;
    }

    public ValidatedSettings? Validated()
        => IsValid() ? new(Url, AuthenticationToken, UtcExpirationDate.Value, RootOrganizationId.Value, MerchantAccountId.Value, IgnoreAcl, IgnoreStoreLimitations) : null;
}

public record ValidatedSettings(
    string Url,
    string Token,
    DateTime Expiration,
    Guid RootOrg,
    Guid Merchant,
    bool IgnoreAcl,
    bool IgnoreStoreLimitations);
