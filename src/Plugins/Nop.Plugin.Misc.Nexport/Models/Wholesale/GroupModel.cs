using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record GroupModel(
    Guid GroupGuid,
    string Name,
    string ShortName
) : BaseNopEntityModel;
