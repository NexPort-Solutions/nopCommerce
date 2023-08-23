using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductRedemptionListSearchModel : BaseSearchModel
    {
        public NexportGroupModel CurrentGroup { get; set; }

        public Product CurrentProduct { get; set; }

        public bool AdminView = false;

        public NexportGroupProductRedemptionListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
