using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public record NexportSupplementalInfoOptionGroupAssociationListSearchModel : BaseSearchModel
    {
        public NexportSupplementalInfoOptionGroupAssociationListSearchModel()
        {
            SetGridPageSize();
        }

        public int OptionId { get; set; }
    }
}
