using System.Data;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IStudentRegistrationFieldAnswerService
{
    Task<List<StudentRegistrationFieldAnswer>> GetAll(int customerId, int fieldId);
    Task<StudentRegistrationFieldAnswer> GetById(int id);
    Task Insert(StudentRegistrationFieldAnswer answer);
    Task Delete(StudentRegistrationFieldAnswer answer);
    Task Update(StudentRegistrationFieldAnswer answer);
    Task BulkUpdateForCustomer(int customerId, int fieldId, Dictionary<string, string> fields);
}

public class StudentRegistrationFieldAnswerService : IStudentRegistrationFieldAnswerService
{
    private readonly IRepository<StudentRegistrationFieldKeyMapping> _studentRegistrationFieldKeyMappings;
    private readonly IRepository<StudentRegistrationFieldAnswer> _studentRegistrationFieldAnswers;

    public StudentRegistrationFieldAnswerService(
        IRepository<StudentRegistrationFieldKeyMapping> studentRegistrationFieldKeyMappings,
        IRepository<StudentRegistrationFieldAnswer> studentRegistrationFieldAnswers)
    {
        _studentRegistrationFieldKeyMappings = studentRegistrationFieldKeyMappings;
        _studentRegistrationFieldAnswers = studentRegistrationFieldAnswers;
    }

    public Task<List<StudentRegistrationFieldAnswer>> GetAll(int customerId, int fieldId)
        => _studentRegistrationFieldAnswers.Table
            .Where(fieldAnswer => fieldAnswer.CustomerId == customerId && fieldAnswer.FieldId == fieldId)
            .ToListAsync();

    public Task<StudentRegistrationFieldAnswer> GetById(int id) => _studentRegistrationFieldAnswers.GetByIdAsync(id);
    public Task Insert(StudentRegistrationFieldAnswer answer) => _studentRegistrationFieldAnswers.InsertAsync(answer);
    public async Task Delete(StudentRegistrationFieldAnswer answer)
    {
        if (await _studentRegistrationFieldAnswers.Table
            .AnyAsync(fieldAnswer =>
                fieldAnswer.CustomerId == answer.CustomerId
                    && fieldAnswer.FieldId == answer.FieldId
                    && fieldAnswer.FieldKey == answer.FieldKey))
        {
            return;
        }
        await _studentRegistrationFieldAnswers.DeleteAsync(answer);
    }

    public Task Update(StudentRegistrationFieldAnswer answer) => _studentRegistrationFieldAnswers.UpdateAsync(answer);

    public async Task BulkUpdateForCustomer(int customerId, int fieldId, Dictionary<string, string> fields)
    {
        var answers = await GetAll(customerId, fieldId);
        var prefix = $"{Defaults.RegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HTML_FIELD_PREFIX}";
        foreach (var (key, value) in fields)
        {
            var prefixEnd = key.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length + 1;
            if (getFieldKey(key[prefixEnd..]) is not { } fieldKey)
            {
                continue;
            }
            if (answers.Find(x => x.FieldKey == fieldKey) is { } currentAnswer)
            {
                currentAnswer.TextValue = value;
                currentAnswer.UtcDateModified = DateTime.UtcNow;
                await Update(currentAnswer);
                return;
            }
            var newAnswer = new StudentRegistrationFieldAnswer
            {
                CustomerId = customerId,
                FieldId = fieldId,
                FieldKey = fieldKey,
                TextValue = value,
                UtcDateCreated = DateTime.UtcNow,
                UtcDateModified = DateTime.UtcNow,
            };
            await Insert(newAnswer);
        }
        string? getFieldKey(string fieldControl) => _studentRegistrationFieldKeyMappings.Table.FirstOrDefault(x => x.FieldKey == fieldControl) is { FieldKey: var fieldKey } ? fieldKey : null;
    }
}
