using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

public class Answer : BaseEntity
{
    public required int CustomerId { get; init; }
    public required int FieldId { get; init; }
    public string? TextValue { get; init; }
    public int? NumericValue { get; init; }
    public DateTime? DateTimeValue { get; init; }
    public bool? BooleanValue { get; init; }
    public required int? FieldOptionId { get; init; }
    public required bool IsCustomField { get; init; }
    public required DateTime UtcDateCreated { get; init; }
    public required DateTime? UtcDateModified { get; init; }
}
