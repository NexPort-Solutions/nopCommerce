using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;

public record ProductStepModel : RedeemStepModel
{
    public IList<SelectListItem> AvailableMappings { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Misc.Nexport.Group.Product.Redemptions.Redeem.SelectProduct")]
    public int? SelectedProductId { get; set; }

    public Guid InvoiceItemId { get; set; }

    public Product CurrentProduct { get; set; }

    public int? ProductMappingIdForOpenEndedProduct { get; set; }

    public bool IsOpenEnded { get; set; }

    public int? RedeemingProductId { get; set; }
}