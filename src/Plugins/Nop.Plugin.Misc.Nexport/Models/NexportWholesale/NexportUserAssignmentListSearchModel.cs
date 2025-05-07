using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
public record NexportUserAssignmentListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemption.RedeemProduct.Customer.SearchNameOrEmail")]
    public string SearchEmail { get; set; }

    public int ProductId { get; set; }

    public Guid? GroupId { get; set; }

    public bool TableFirstDraw { get; set; } = true;

    public NexportUserAssignmentListSearchModel()
    {
        SetGridPageSize();
    }
}
