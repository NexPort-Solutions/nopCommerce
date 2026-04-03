using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record NexportRegisterModel : RegisterModel
{
    public NexportNavigationContextModel NavigationContext { get; set; } = new();
}
