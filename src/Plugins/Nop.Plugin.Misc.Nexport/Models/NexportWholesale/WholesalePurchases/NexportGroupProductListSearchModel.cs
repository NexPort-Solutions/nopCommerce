using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases
{
    public record NexportGroupProductListSearchModel : BaseSearchModel
    {
        public bool AdminView { get; set; } = false;

        public bool HasPurchasingAgentPermissions { get; set; } = false;

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.SearchGroupName")]
        public string SearchGroupName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.SearchGroupShortName")]
        public string SearchGroupShortName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.SearchProductName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchStatus")]
        public NexportOrderInvoiceItemRedemptionStatus? SearchStatusId { get; set; }


        public IList<SelectListItem> AvailableStatuses { get; set; }

        public NexportGroupProductListSearchModel()
        {
            SetGridPageSize();

            AvailableStatuses = new List<SelectListItem>();
        }
    }
}
