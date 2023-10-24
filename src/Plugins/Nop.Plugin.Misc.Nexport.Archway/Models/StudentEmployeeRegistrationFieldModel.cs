using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record StudentEmployeeRegistrationFieldModel : BaseNopModel
{
    public int FieldId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationState")]
    public required string StoreLocationState { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationCity")]
    public required string StoreLocationCity { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.StoreLocationAddress")]
    public required string StoreLocationAddress { get; set; }

    public required int StoreNumber { get; set; }

    public required string StoreType { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.EmployeePosition")]
    public required string EmployeePosition { get; set; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Field.EmployeeId")]
    public required string EmployeeId { get; set; }
}
