using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Syllabus;

public record SyllabusListSearchModel : BaseSearchModel
{
    public SyllabusListSearchModel() => SetGridPageSize();

    public Guid CatalogId { get; set; }

    public Guid ProductId { get; set; }

    public int NopProductId { get; set; }
}
