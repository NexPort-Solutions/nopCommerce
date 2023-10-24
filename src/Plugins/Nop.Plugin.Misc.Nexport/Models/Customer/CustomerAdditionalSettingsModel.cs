using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record CustomerAdditionalSettingsModel : BaseNopModel
{
    public CategorySearchModel RegistrationFieldCategorySearchModel { get; set; } = new();
    public SearchModel RegistrationFieldSearchModel { get; set; } = new();
}
