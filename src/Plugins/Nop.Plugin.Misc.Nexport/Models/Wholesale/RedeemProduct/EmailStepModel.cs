
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct
{
    public record EmailStepModel : BaseNopEntityModel
    {
        public string? Email { get; set; }

        public bool EmailStepSendViaEmail { get; set; }
    }
}
