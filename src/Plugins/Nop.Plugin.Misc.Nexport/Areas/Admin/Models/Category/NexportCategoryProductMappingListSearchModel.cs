using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category;

public record NexportCategoryProductMappingListSearchModel : BaseSearchModel
{
    public NexportCategoryProductMappingListSearchModel()
    {
        SetGridPageSize();
    }

    public int NopCategoryId { get; set; }

}