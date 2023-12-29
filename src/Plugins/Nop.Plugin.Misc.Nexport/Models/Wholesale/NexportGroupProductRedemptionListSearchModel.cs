using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    public record NexportGroupProductRedemptionListSearchModel : BaseSearchModel
    {
        public NexportGroupModel CurrentGroup { get; set; } = new NexportGroupModel{Id = null,Name="No Group"};

        public Product CurrentProduct { get; set; }

        public bool AdminView = false;

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.SearchStatus")]
        public NexportOrderInvoiceItemRedemptionStatus? SearchStatusId { get; set; }


        public IList<SelectListItem> AvailableStatuses { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedFrom")]
        [UIHint("DateNullable")]
        public DateTime? DateAssignedFrom { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.DateAssignedTo")]
        [UIHint("DateNullable")]
        public DateTime? DateAssignedTo { get; set; }


        public NexportGroupProductRedemptionListSearchModel()
        {
            AvailableStatuses = new List<SelectListItem>();

            SetGridPageSize();
        }
    }
}
