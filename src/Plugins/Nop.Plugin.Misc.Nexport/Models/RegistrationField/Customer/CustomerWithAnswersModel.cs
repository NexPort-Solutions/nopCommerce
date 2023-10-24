using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;

public record CustomerWithAnswersModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }
    public string? FieldName { get; set; }
    public string? FieldType { get; set; }
    public string? CustomProfileFieldKey { get; set; }
    public string? CustomRender { get; set; }
}
