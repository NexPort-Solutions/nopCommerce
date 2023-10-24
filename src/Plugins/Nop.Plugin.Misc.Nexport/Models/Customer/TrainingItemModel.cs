using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Models.Customer;

public class TrainingItemModel
{
    public string? Name { get; set; }
    public Guid SyllabusId { get; set; }
    public InvoiceRedemptionResponse.RedemptionTypeEnum Type { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public DateTime? UtcStartDate { get; set; }
    public DateTime? UtcExpirationDate { get; set; }
    public DateTime? UtcRedemptionDate { get; set; }
    public Enums.PhaseEnum Status { get; set; }
}
