using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public record CustomerAdditionalInfoModel : BaseNopModel
{
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.UserId")]
    public Guid? UserId { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.UserFullName")]
    public string? UserFullName { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgId")]
    public Guid? OwnerOrgId { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgName")]
    public string? OwnerOrgName { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgShortName")]
    public string? OwnerOrgShortName { get; set; }

    [NopResourceDisplayName("Account.Fields.Nexport.Email")]
    public string? Email { get; set; }

    public SupplementalInfoAnswerListSearchModel? SupplementalInfoAnswerListSearchModel { get; set; }
    public CustomerSupplementalInfoAnsweredQuestionListSearchModel? CustomerSupplementalInfoAnsweredQuestionListSearchModel { get; set; }
    public ListCustomerAnswersSearchModel? ListCustomerAnswersSearchModel { get; set; }
    public CustomerAnswersListSearchModel? CustomerAnswersListSearchModel { get; set; }
    public bool Editable { get; set; }
}
