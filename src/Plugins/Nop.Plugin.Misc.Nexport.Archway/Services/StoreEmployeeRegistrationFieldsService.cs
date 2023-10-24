using System.Data;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Localization;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IStoreEmployeeRegistrationFieldsService
{
    Dictionary<string, string> Parse(int fieldId, IFormCollection form);
    Task Save(Customer customer, int fieldId, Dictionary<string, string> fields);
    Task<Dictionary<string, string>> Process(int customerId, int fieldId);
    Task<Dictionary<string, string>> GetById(int customerId, int fieldId);
}

public class StoreEmployeeRegistrationFieldsService : IStoreEmployeeRegistrationFieldsService
{
    private readonly IRepository<StudentRegistrationFieldKeyMapping> _studentRegistrationFieldKeyMappings;
    private readonly IRepository<StudentRegistrationFieldAnswer> _studentRegistrationFieldAnswers;
    private readonly IStudentRegistrationFieldKeyMappingService _studentRegistrationFieldKeyMappingService;
    private readonly IStudentRegistrationFieldAnswerService _studentRegistrationFieldAnswerService;
    private readonly ILocalizationService _localizationService;
    private readonly IRegistrationFieldService _registrationField;

    public StoreEmployeeRegistrationFieldsService(
        IRepository<StudentRegistrationFieldKeyMapping> studentRegistrationFieldKeyMapping,
        IRepository<StudentRegistrationFieldAnswer> studentRegistrationFieldAnswers,
        IStudentRegistrationFieldKeyMappingService studentRegistrationFieldKeyMappingService,
        IStudentRegistrationFieldAnswerService studentRegistrationFieldAnswerService,
        ILocalizationService localizationService,
        IRegistrationFieldService registrationField)
    {
        _studentRegistrationFieldKeyMappings = studentRegistrationFieldKeyMapping;
        _studentRegistrationFieldAnswers = studentRegistrationFieldAnswers;
        _studentRegistrationFieldKeyMappingService = studentRegistrationFieldKeyMappingService;
        _studentRegistrationFieldAnswerService = studentRegistrationFieldAnswerService;
        _localizationService = localizationService;
        _registrationField = registrationField;
    }

    public Dictionary<string, string> Parse(int fieldId, IFormCollection form)
    {
        var controlId = $"{Defaults.RegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HTML_FIELD_PREFIX}";
        var customFieldsInForm = form.Where(formValue => formValue.Key.Contains(controlId, StringComparison.OrdinalIgnoreCase));
        return customFieldsInForm.ToDictionary(
            fieldValue => fieldValue.Key[(fieldValue.Key.IndexOf(PluginDefaults.HTML_FIELD_PREFIX, StringComparison.Ordinal) + PluginDefaults.HTML_FIELD_PREFIX.Length + 1)..],
            fieldValue => fieldValue.Value.ToString());
    }

    public async Task Save(Customer customer, int fieldId, Dictionary<string, string> fields)
    {
        if (!fields.TryGetValue(nameof(StudentEmployeeRegistrationFieldModel.EmployeePosition), out var employeePositionField) || string.IsNullOrWhiteSpace(employeePositionField))
        {
            return;
        }
        var answer = new Answer
        {
            CustomerId = customer.Id,
            FieldId = fieldId,
            IsCustomField = true,
            UtcDateCreated = DateTime.UtcNow,
            UtcDateModified = DateTime.UtcNow,
            FieldOptionId = null,
        };
        await _registrationField.InsertAnswer(answer);
        var answers = fields.FilterMapAwaitAsync(async field => await _studentRegistrationFieldKeyMappingService.GetByKey(field.Key) is { FieldKey: var key }
            ? new StudentRegistrationFieldAnswer
            {
                CustomerId = customer.Id,
                FieldId = fieldId,
                FieldKey = key,
                TextValue = field.Value,
                UtcDateCreated = DateTime.UtcNow,
                UtcDateModified = DateTime.UtcNow,
            }
            : null);
        await foreach (var studentAnswer in answers)
        {
            await _studentRegistrationFieldAnswerService.Insert(studentAnswer);
        }
    }

    public Task<Dictionary<string, string>> Process(int customerId, int fieldId)
        => _studentRegistrationFieldAnswers.Table
            .Where(x => x.CustomerId == customerId && x.FieldId == fieldId)
            .ToDictionaryAsync(answer => answer.FieldKey, answer => answer.TextValue);

    public async Task<Dictionary<string, string>> GetById(int customerId, int fieldId)
    {
        var result = new Dictionary<string, string>();
        var answers = await _studentRegistrationFieldAnswers.Table
            .Where(fieldAnswer => fieldAnswer.CustomerId == customerId && fieldAnswer.FieldId == fieldId)
            .Where(x => x.FieldKey != "StoreIdField" && x.FieldKey != "StoreTypeField")
            .ToListAsync();
        foreach (var answer in answers)
        {
            var fieldKeyInfo = _studentRegistrationFieldKeyMappings.Table.FirstOrDefault(x => x.FieldKey == answer.FieldKey);
            if (fieldKeyInfo is not null)
            {
                result.Add(await _localizationService.GetResourceAsync($"Plugins.Misc.Nexport.Archway.Field.{fieldKeyInfo.FieldControlName}"), answer.TextValue);
            }
        }

        return result;
    }
}
