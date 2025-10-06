using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record ArchwayPluginResourceListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
    public string FriendlyName { get; set; }
}
