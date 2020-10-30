using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order
{
    public class NexportOrderInvoiceItemModel : BaseNopEntityModel
    {
        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

        public string ProductName { get; set; }

        public string NexportProductName { get; set; }

        public Guid NexportSyllabusId { get; set; }

        public Guid ExistingEnrollmentId { get; set; }

        public DateTime? UtcExistingEnrollmentExpirationDate { get; set; }

        public Guid InvoiceItemId { get; set; }

        public Guid InvoiceId { get; set; }

        public bool? RequireManualApproval { get; set; }
    }
}
