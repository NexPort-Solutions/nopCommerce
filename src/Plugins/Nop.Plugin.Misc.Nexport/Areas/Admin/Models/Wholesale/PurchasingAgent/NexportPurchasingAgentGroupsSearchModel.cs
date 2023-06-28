using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Wholesale.PurchasingAgent
{
    public record NexportPurchasingAgentGroupsSearchModel : BaseSearchModel
    {

        public NexportPurchasingAgentGroupsSearchModel()
        {
            SetGridPageSize();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PurchasingAgentGroups.SearchOrgName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PurchasingAgentGroups.SearchOrgShortName")]
        public string SearchShortName { get; set; }
    }
}
