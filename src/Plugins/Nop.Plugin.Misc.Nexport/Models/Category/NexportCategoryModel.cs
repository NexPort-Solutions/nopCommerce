using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Category
{
    public class NexportCategoryModel : CategoryModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase")]
        public bool LimitSingleProductPurchase { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase")]
        public bool AutoSwapProductPurchase { get; set; }
    }
}
