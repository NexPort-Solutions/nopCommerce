using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportUserMappingModel : BaseNopModel
    {
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

        public bool Editable { get; set; }
    }
}
