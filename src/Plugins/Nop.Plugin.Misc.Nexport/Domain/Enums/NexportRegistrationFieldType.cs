using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums
{
    public enum NexportRegistrationFieldType
    {
        None = 0,

        [Display(Name = "Text Field")]
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
        CustomType = 1000
    }
}
