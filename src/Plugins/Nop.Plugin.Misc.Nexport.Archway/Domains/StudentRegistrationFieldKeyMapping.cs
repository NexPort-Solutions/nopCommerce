using System.Diagnostics.CodeAnalysis;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Archway.Domains;

public class StudentRegistrationFieldKeyMapping : BaseEntity
{
    [SetsRequiredMembers]
    public StudentRegistrationFieldKeyMapping(string fieldControlName, string fieldKey)
    {
        FieldControlName = fieldControlName;
        FieldKey = fieldKey;
    }

    public required string FieldControlName { get; init; }
    public required string FieldKey { get; init; }
}
