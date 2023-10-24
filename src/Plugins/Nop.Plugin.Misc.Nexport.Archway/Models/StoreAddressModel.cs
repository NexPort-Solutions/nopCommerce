using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record StoreAddressModel : BaseNopModel
{
    public required int StoreNumber { get; set; }
    public required string Name { get; set; }
    public required string StoreType { get; set; }
}
