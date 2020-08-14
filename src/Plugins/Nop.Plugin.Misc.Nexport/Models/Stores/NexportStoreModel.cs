using System;
using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Stores
{
    public class NexportStoreModel : StoreModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.NexportSubscriptionOrgId")]
        [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/GuidNullable.cshtml")]
        public Guid? NexportSubscriptionOrgId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SaleModel")]
        public NexportStoreSaleModel? SaleModel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses")]
        public bool AllowRepurchaseFailedCourses { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AllowRepurchasePassedCourses")]
        public bool AllowRepurchasePassedCourses { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.HideSectionCEUsInProductPage")]
        public bool HideSectionCEUsInProductPage { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts")]
        public bool HideAddToCartForIneligibleProducts { get; set; }
    }
}
