using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Wholesale.PurchasingAgent
{
    public record NexportPurchasingAgentSearchModel : BaseSearchModel
    {

        public NexportPurchasingAgentSearchModel()
        {
            SetGridPageSize();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PurchasingAgent.SearchAgentEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PurchasingAgent.SearchAgentName")]
        public string SearchName { get; set; }
    }
}
