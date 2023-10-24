using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record AddCustomerAdditionalInfoModel : BaseNopModel
{
    public required int CustomerId { get; init; }
    public required int StoreId { get; init; }
    public required CustomerAnswersListSearchModel? CustomerRegistrationFieldAnswerListSearchModel { get; init; }
}
