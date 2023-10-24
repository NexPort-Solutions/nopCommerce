using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;

public record CustomerAnswersListSearchModel : BaseSearchModel
{
    public CustomerAnswersListSearchModel() => SetGridPageSize();

    public required int CustomerId { get; set; }
    public int? FieldId { get; set; }
}
