using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportProductGroupMembershipMappingModel : BaseNopEntityModel
    {
        public int NexportProductMappingId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupId")]
        public Guid NexportGroupId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupName")]
        public string NexportGroupName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.GroupShortName")]
        public string NexportGroupShortName { get; set; }
    }
}
