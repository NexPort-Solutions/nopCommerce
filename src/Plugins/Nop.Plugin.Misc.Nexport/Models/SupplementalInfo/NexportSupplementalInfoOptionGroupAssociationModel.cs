using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.SupplementalInfo
{
    public class NexportSupplementalInfoOptionGroupAssociationModel : BaseNopEntityModel
    {
        public int NexportProductMappingId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupId")]
        public Guid NexportGroupId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupName")]
        public string NexportGroupName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupShortName")]
        public string NexportGroupShortName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Active")]
        public bool IsActive { get; set; }
    }
}
