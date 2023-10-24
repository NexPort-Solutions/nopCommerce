using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Stores;

public record StoreModel : Web.Areas.Admin.Models.Stores.StoreModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgId")]
    [UIHint("~/Plugins/Misc.Nexport/Views/Shared/EditorTemplates/GuidNullable.cshtml")]
    public Guid? SubscriptionOrgId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.SaleModel")]
    public StoreSaleModel? SaleModel { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AllowRepurchaseFailedCourses")]
    public bool AllowRepurchaseFailedCourses { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.AllowRepurchasePassedCourses")]
    public bool AllowRepurchasePassedCourses { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.HideSectionCEUsInProductPage")]
    public bool HideSectionCEUsInProductPage { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.HideAddToCartForIneligibleProducts")]
    public bool HideAddToCartForIneligibleProducts { get; set; }
}
