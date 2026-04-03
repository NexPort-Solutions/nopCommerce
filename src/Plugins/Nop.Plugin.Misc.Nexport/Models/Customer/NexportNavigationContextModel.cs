namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record NexportNavigationContextModel
{
    public string LoginRouteName { get; init; } = NexportDefaults.NexportLoginRouteName;

    public string RegisterRouteName { get; init; } = NexportDefaults.NexportRegistrationRouteName;

    public string ReturnUrlParameterName { get; init; } = NexportDefaults.DefaultReturnUrlParameterName;

    public string SafeReturnUrl { get; init; } = string.Empty;
}
