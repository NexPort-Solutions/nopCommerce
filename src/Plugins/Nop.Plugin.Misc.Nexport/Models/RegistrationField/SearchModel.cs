using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField;

public record SearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.CustomerSettings.RegistrationFields.SearchStoreId")]
    public IList<int> SelectedStoreIds { get; init; } = new List<int>();

    public IList<SelectListItem> AvailableStores { get; init; } = new List<SelectListItem>();
}
