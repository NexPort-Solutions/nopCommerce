using Microsoft.AspNetCore.Http;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;

public record EditCustomerAnswersModel : BaseNopModel
{
    public RegistrationFieldModel? RegistrationField { get; set; }
    public IList<Option> Options { get; init; } = new List<Option>();
    public IList<Answer> Answers { get; init; } = new List<Answer>();
}

[Serializable]
public record EditCustomerAnswersRequest
{
    public int FieldId { get; set; }
    public bool? AllowMultipleSelection { get; set; }
    public string? AnswerValue { get; set; }
    public IList<int> PreviousAnswers { get; init; } = new List<int>();
    public IList<int> AnswerFieldOptions { get; init; } = new List<int>();
    public IFormCollection FormCollection { get; init; } = new FormCollection(default);
}
