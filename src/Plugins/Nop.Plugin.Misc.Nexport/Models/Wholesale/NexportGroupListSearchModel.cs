using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupListSearchModel : BaseSearchModel
    {
        public NexportGroupListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
