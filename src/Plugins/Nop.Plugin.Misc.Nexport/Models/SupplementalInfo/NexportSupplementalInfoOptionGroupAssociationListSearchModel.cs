using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
