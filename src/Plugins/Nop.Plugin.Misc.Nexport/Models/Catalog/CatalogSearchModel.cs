using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Catalog;

public record CatalogSearchModel : BaseSearchModel
{
    public CatalogSearchModel() => SetGridPageSize();

    public Guid? OrgId { get; set; }
    public int NopProductId { get; set; }
    public SyllabusListSearchModel SyllabusListSearch { get; set; } = new();
}
