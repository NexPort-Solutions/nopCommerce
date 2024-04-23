using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases
{
    /// <summary>
    /// Represents a category list model
    /// </summary>
    public partial record NexportRedemptionRequestUnassignmentListModel : BasePagedListModel<NexportRedemptionUnassignmentRequestModel>
    {
    }
}