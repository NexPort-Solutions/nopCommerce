using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Category;

public record CategoryModel : Web.Areas.Admin.Models.Catalog.CategoryModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Category.LimitSingleProductPurchase")]
    public bool LimitSingleProductPurchase { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Category.AutoSwapProductPurchase")]
    public bool AutoSwapProductPurchase { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Category.AllowProductPurchaseInCategoryDuringEnrollment")]
    public bool AllowProductPurchaseInCategoryDuringEnrollment { get; set; }
}
