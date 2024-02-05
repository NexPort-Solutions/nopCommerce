using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct
{
    public record CustomerStepListSearchModel : BaseSearchModel
    {
        public string? SearchEmail { get; set; }

        public string? UserLinkUri { get; set; }
        
        public Guid? SelectedUserId { get; set; }

        public string? SelectedUserFirstName { get; set; }
        
        public string? SelectedUserLastName { get; set; }

        public bool CustomerStepSendViaEmail { get; set; }

        public CustomerStepListSearchModel()
        {
            SetGridPageSize();
        }
    }
}
