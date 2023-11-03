using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductListSearchModel : BaseSearchModel
    {
        public NexportGroupModel CurrentGroup { get; set; } =
            new NexportGroupModel {Id = null, Name = "No Group"};

        public bool AdminView = false;

        public NexportGroupProductListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
