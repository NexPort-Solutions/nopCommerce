using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record StoreEmployeePositionModel : BaseNopModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}
