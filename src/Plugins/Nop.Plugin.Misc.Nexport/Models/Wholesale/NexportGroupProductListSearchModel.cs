using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductListSearchModel : BaseSearchModel
    {
        public NexportGroupModel CurrentGroup { get; set; }

        public bool AdminView = false;

        public NexportGroupProductListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
