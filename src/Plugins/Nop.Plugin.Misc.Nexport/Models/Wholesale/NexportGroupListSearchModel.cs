using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupListSearchModel : BaseSearchModel
    {
        public bool AdminView = false;

        public NexportGroupListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
