﻿using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportProductGroupMembershipMappingListSearchModel : BaseSearchModel
    {
        public NexportProductGroupMembershipMappingListSearchModel()
        {
            SetGridPageSize();
        }

        public int NexportProductMappingId { get; set; }
    }
}
