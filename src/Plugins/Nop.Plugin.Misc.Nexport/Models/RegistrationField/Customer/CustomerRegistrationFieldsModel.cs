namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;

public record CustomerAnswersModel
{
    public IDictionary<CategoryModel, List<RegistrationFieldModel>> RegistrationFieldsWithCategory { get; init; } = new Dictionary<CategoryModel, List<RegistrationFieldModel>>();
    public IList<RegistrationFieldModel> RegistrationFieldsWithoutCategory { get; init; } = new List<RegistrationFieldModel>();
}
