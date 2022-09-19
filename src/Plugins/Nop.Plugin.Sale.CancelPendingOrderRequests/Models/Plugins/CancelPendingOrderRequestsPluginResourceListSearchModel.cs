using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models.Plugins;

public record CancelPendingOrderRequestsPluginResourceListSearchModel() : BaseSearchModel
{
    [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
    public string FriendlyName { get; set; }
}