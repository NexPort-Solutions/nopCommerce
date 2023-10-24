using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models;

public record StudentEmployeeRegistrationFieldOptionModel : BaseNopModel
{
    [FromForm(Name = "GroupGuid")]
    public required int FieldId { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreStateFieldKey")]
    public required string StoreStateFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreCityFieldKey")]
    public required string StoreCityFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreAddressFieldKey")]
    public required string StoreAddressFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreIdFieldKey")]
    public required string StoreIdFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.StoreTypeFieldKey")]
    public required string StoreTypeFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeeIdFieldKey")]
    public required string EmployeeIdFieldKey { get; init; }

    [NopResourceDisplayName("Plugins.Misc.Nexport.Archway.Fields.Options.EmployeePositionFieldKey")]
    public required string EmployeePositionFieldKey { get; init; }
}
