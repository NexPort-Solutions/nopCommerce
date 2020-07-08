using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class NexportCustomerAdditionalInfoModel : BaseNopModel
    {
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Account.Fields.Nexport.UserId")]
        public Guid? NexportUserId { get; set; }

        [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgId")]
        public Guid? OwnerOrgId { get; set; }

        [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgName")]
        public string OwnerOrgName { get; set; }

        [NopResourceDisplayName("Account.Fields.Nexport.OwnerOrgShortName")]
        public string OwnerOrgShortName { get; set; }

        [NopResourceDisplayName("Account.Fields.Nexport.Email")]
        public string NexportEmail { get; set; }

        public NexportSupplementalInfoAnswerListSearchModel NexportSupplementalInfoAnswerListSearchModel { get; set; }

        public NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel { get; set; }

        public bool Editable { get; set; }
    }
}
