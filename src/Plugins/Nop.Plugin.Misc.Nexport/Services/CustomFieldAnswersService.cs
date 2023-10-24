using Nop.Services.Plugins;
using static Nop.Plugin.Misc.Nexport.Domain.RegistrationField.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using System.Globalization;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ICustomFieldAnswersService
{
    Task<Dictionary<string, string>> ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(IEnumerable<Answer> fieldAnswers);
    Task<Dictionary<string, string>> ConvertFieldAnswersToSubmissionProfileFields(IEnumerable<Answer> fieldAnswers);
}

public class CustomFieldAnswersService : ICustomFieldAnswersService
{
    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly IRegistrationFieldService _registrationField;

    public CustomFieldAnswersService(
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        IRegistrationFieldService registrationField)
    {
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _registrationField = registrationField;
    }

    public async Task<Dictionary<string, string>> ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(IEnumerable<Answer> fieldAnswers)
    {
        // todo fix horribly inefficient
        var result = new Dictionary<string, string>();
        foreach (var answer in fieldAnswers)
        {
            if (await _registrationField.GetById(answer.FieldId) is not { Type: RegistrationFieldType.CustomType, CustomFieldRender: var customFieldRender, Id: var id }
                || await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(customFieldRender) is not { } customRender)
            {
                continue;
            }
            if (await customRender.ProcessCustomRegistrationFields(answer.CustomerId, id) is { } processResult)
            {
                result = result.Concat(processResult).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            }
        }
        return result;
    }

    public virtual async Task<Dictionary<string, string>> ConvertFieldAnswersToSubmissionProfileFields(IEnumerable<Answer> fieldAnswers)
    {
        var result = new Dictionary<string, string>();
        var fieldAnswersByFieldId = fieldAnswers
            .GroupBy(answer => answer.FieldId)
            .ToDictionary(group => group.Key, group => group.ToList());
        foreach (var item in fieldAnswersByFieldId)
        {
            if (await _registrationField.GetById(item.Key) is not { Type: var type, CustomProfileFieldKey: var customProfileFieldKey }
                || string.IsNullOrWhiteSpace(customProfileFieldKey)
                || type is RegistrationFieldType.CustomType)
            {
                continue;
            }
            string fieldValue;
            if (type is RegistrationFieldType.SelectCheckbox or RegistrationFieldType.SelectDropDown)
            {
                var answerValues = await item.Value.Where(answer => answer.FieldOptionId is not null)
                    .SelectAwait(async answer => await _registrationField.GetOptionById(answer.FieldOptionId!.Value))
                    .WhereNotNull()
                    .Select(fieldOption => fieldOption.OptionValue)
                    .ToListAsync();
                fieldValue = string.Join(',', answerValues);
            }
            else if (item.Value.FirstOrDefault() is not Answer answer)
            {
                continue;
            }
            else if (!string.IsNullOrEmpty(answer.TextValue))
            {
                fieldValue = answer.TextValue;
            }
            else if (answer.NumericValue is not null)
            {
                fieldValue = answer.NumericValue.Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (answer.BooleanValue is not null)
            {
                fieldValue = answer.BooleanValue.Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (answer.DateTimeValue is not null)
            {
                fieldValue = answer.DateTimeValue.Value.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException($"Invalid answer {answer}");
            }
            result.Add(customProfileFieldKey, fieldValue);
        }
        return result;
    }
}
