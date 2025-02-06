using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record SubmitInvoiceItemRefundRequestModel : BaseNopModel
{
    public Guid InvoiceItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.InvoiceItemRefund.Request.Comments")]
    public string Comments { get; set; }

    public string Result { get; set; }
}