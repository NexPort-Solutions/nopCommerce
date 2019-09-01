using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportGetInvoiceResponseDetails : NexportApiResponseBase
    {
        public GetInvoiceResponse Response { get; set; }
    }
}
