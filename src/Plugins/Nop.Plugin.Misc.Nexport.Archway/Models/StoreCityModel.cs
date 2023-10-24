using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record StoreCityModel : BaseNopModel
{
    public required string Name { get; set; }
}
