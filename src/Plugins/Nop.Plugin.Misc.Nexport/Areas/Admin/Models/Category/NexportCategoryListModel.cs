using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Category
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record NexportCategoryListModel : BasePagedListModel<NexportCategoryModel>
    {
    }
}