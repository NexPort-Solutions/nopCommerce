using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale;

public record RedeemProductOrModifyProductRedemptionModel : BaseNopEntityModel
{
    public bool AdminView { get; init; }
    public GroupModel? CurrentGroup { get; init; }
    public Product? CurrentProduct { get; init; }
    public Guid? InvoiceItemId { get; init; }
    public bool HasBeenAssigned { get; init; }
}
