using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField;

public record OptionModel : BaseNopEntityModel
{
    public int FieldId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Option.Value")]
    public string? OptionValue { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Option.DisplayOrder")]
    public int DisplayOrder { get; set; }
}
