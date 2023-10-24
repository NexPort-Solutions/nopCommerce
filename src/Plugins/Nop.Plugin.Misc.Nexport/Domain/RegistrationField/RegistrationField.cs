using System.ComponentModel.DataAnnotations;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

public class RegistrationField : BaseEntity, ILocalizedEntity
{
    public const string RENDER_N_A = "N/A";

    public required string? Name { get; set; }
    public required RegistrationFieldType Type { get; set; }
    public required string? CustomProfileFieldKey { get; set; }
    public required bool IsRequired { get; set; }
    public required bool IsActive { get; set; }
    public required bool Validation { get; set; }
    public required string? ValidationRegex { get; set; }
    public required string? ValidationMessage { get; set; }
    public required int? FieldCategoryId { get; set; }
    public required int DisplayOrder { get; set; }
    public required string? CustomFieldRender { get; set; }

    public enum RegistrationFieldType
    {
        None = 0,

        [Display(Name = "NONE Field")]
        Text = 1,

        [Display(Name = "Date/Time Field")]
        DateTime = 2,

        [Display(Name = "Yes/No Field")]
        Boolean = 3,

        [Display(Name = "Number Field")]
        Numeric = 4,

        [Display(Name = "Email Field")]
        Email = 5,

        [Display(Name = "Select Dropdown Field")]
        SelectDropDown = 6,

        [Display(Name = "Date Field")]
        DateOnly = 7,

        [Display(Name = "Select Checkbox Field")]
        SelectCheckbox = 8,

        [Display(Name = "Custom Field")]
        CustomType = 1000,
    }
}
