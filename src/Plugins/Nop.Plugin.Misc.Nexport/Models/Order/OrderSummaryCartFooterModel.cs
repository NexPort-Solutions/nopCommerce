using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public record OrderSummaryCartFooterModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.Order.GroupId")]
        public Guid? PurchasingGroupId { get; set; }

        public NexportGroupModel PurchasingGroup { get; set; }

        public int? FundingPoolId { get; set; }

        public NexportFundingPoolModel FundingPool { get; set; }

        public bool ShowPurchasingGroupArea { get; set; } = true;

        public bool TogglePurchasingGroupText { get; set; } = true;

        public IList<NexportGroupModel> AvailableGroups { get; set; } = new List<NexportGroupModel>();

        public IList<NexportFundingPoolModel> AvailableFundingPools { get; set; } = new List<NexportFundingPoolModel>();
    }
}