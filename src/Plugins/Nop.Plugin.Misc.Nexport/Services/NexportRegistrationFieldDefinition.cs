using Nop.Core.Domain.Common;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Services;

public partial class NexportRegistrationFieldDefinition
{
    public IList<NexportRegistrationField> Fields { get; init; } = new List<NexportRegistrationField>();

    public IList<NexportRegistrationFieldCategory> Categories { get; init; } = new List<NexportRegistrationFieldCategory>();

    public IList<NexportRegistrationFieldOption> Options { get; init; } = new List<NexportRegistrationFieldOption>();

    public IList<GenericAttribute> Attributes { get; init; } = new List<GenericAttribute>();
}
