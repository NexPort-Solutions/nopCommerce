using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportUserMappingModel : BaseNopModel
    {
        [NopResourceDisplayName("Account.Fields.Nexport.UserId")]
        public Guid? NexportUserId { get; set; }

        public Guid? OwnerOrgId { get; set; }

        public string OwnerOrgShortName { get; set; }

        public string NexportEmail { get; set; }
    }
}
