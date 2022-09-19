using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Models.Plugins;

public record PurchaseForCustomerPluginResourceListSearchModel : BaseSearchModel 
{
    [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
    public string FriendlyName { get; set; }
}