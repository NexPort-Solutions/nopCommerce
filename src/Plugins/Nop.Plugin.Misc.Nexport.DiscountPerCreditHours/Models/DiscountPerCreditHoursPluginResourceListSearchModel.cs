using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models;

public record DiscountPerCreditHoursPluginResourceListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
    public string FriendlyName { get; set; }
}