using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public record OrderSummaryCartFooterModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.Order.GroupId")]
        public Guid GroupGuid { get; set; }

        public NexportGroupModel? Group { get; set; }

        public bool ShowPurchasingGroupArea { get; set; } = true;

        public string PurchasingGroupSelectBoxStyle { get; set; } = "";

        public IList<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();
        public IList<NexportGroupModel> AvailableGroups2 { get; set; } = new List<NexportGroupModel>();
    }
}
