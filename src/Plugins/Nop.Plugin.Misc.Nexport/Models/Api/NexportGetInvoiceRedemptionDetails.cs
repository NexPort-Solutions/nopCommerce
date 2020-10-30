using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models.Api
{
    public class NexportGetInvoiceRedemptionDetails : NexportApiResponseBase
    {
        public InvoiceRedemptionResponse Response { get; set; }
    }
}