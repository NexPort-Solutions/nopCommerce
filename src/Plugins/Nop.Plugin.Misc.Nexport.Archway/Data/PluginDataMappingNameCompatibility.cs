using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Data;

public class PluginDataMappingNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(StudentRegistrationFieldKeyMapping), "ArchwayStudentRegistrationFieldKeyMapping" },
        { typeof(StudentRegistrationFieldAnswer), "ArchwayStudentRegistrationFieldAnswer" },
        { typeof(StoreEmployeePosition), "ArchwayStoreEmployeePosition" },
        { typeof(StoreRecordInfo), "ArchwayStore" },
    };

    public Dictionary<(Type, string), string> ColumnName => new();
}
