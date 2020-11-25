using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportSupplementalInfoOptionGroupAssociationListSearchModel : BaseSearchModel
    {
        public NexportSupplementalInfoOptionGroupAssociationListSearchModel()
        {
            SetGridPageSize();
        }

        public int OptionId { get; set; }
    }
}
