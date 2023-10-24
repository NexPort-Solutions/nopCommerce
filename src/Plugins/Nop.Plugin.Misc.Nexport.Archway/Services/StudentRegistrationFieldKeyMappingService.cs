using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IStudentRegistrationFieldKeyMappingService
{
    Task<StudentRegistrationFieldKeyMapping?> GetByName(string fieldControlName);
    Task<StudentRegistrationFieldKeyMapping?> GetByKey(string fieldKey);
    Task InsertOrUpdate(StudentRegistrationFieldKeyMapping fieldKeyMapping);
    Task Delete(StudentRegistrationFieldKeyMapping fieldKeyMapping);
}

public class StudentRegistrationFieldKeyMappingService : IStudentRegistrationFieldKeyMappingService
{
    private readonly IRepository<StudentRegistrationFieldKeyMapping> _fieldKeyMappings;

    public StudentRegistrationFieldKeyMappingService(IRepository<StudentRegistrationFieldKeyMapping> studentRegistrationFieldKeyMappings)
        => _fieldKeyMappings = studentRegistrationFieldKeyMappings;

    public Task<StudentRegistrationFieldKeyMapping?> GetByName(string fieldControlName)
        => _fieldKeyMappings.Table.FirstOrDefaultAsync(mapping => mapping.FieldControlName == fieldControlName)!; // FirstOrDefaultAsync can return null.

    public Task<StudentRegistrationFieldKeyMapping?> GetByKey(string fieldKey)
        => _fieldKeyMappings.Table.FirstOrDefaultAsync(mapping => mapping.FieldKey == fieldKey)!; // FirstOrDefaultAsync can return null.

    public Task InsertOrUpdate(StudentRegistrationFieldKeyMapping fieldKeyMapping)
    {
        if (_fieldKeyMappings.Table.FirstOrDefault(mapping => mapping.FieldControlName == mapping.FieldControlName) is { } currentMapping)
        {
            var updated = new StudentRegistrationFieldKeyMapping(currentMapping.FieldControlName, fieldKeyMapping.FieldKey);
            return _fieldKeyMappings.UpdateAsync(updated);
        }
        return _fieldKeyMappings.InsertAsync(fieldKeyMapping);
    }

    public Task Delete(StudentRegistrationFieldKeyMapping fieldKeyMapping) => _fieldKeyMappings.DeleteAsync(fieldKeyMapping);
    public Task Update(StudentRegistrationFieldKeyMapping fieldKeyMapping) => _fieldKeyMappings.UpdateAsync(fieldKeyMapping);
}
