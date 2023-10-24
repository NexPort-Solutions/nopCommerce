using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public record InvoiceItemModel : BaseNopEntityModel
{
    public required int OrderId { get; init; }
    public required int OrderItemId { get; init; }
    public required string? ProductName { get; set; }
    public required string? NexportProductName { get; set; }
    public required Guid? SyllabusId { get; set; }
    public required Guid? ExistingEnrollmentId { get; set; }
    public required DateTime? UtcExistingEnrollmentExpirationDate { get; set; }
    public required Guid? InvoiceItemId { get; init; }
    public required Guid? InvoiceId { get; init; }
    public required bool? RequireManualApproval { get; init; }
}
