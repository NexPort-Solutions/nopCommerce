using NexportApi.Model;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportService
{
    Task GenerateNewTokenAsync(string url, string username, string password, bool tokenNeverExpired, DateTime? tokenExpirationDate);
    Task<List<DirectoryResponseItem>> SearchDirectory(string searchTerm, int? page = null);
    Task<SetCustomProfileFieldValuesResponse?> SetCustomProfileFieldValues(Guid subscriberId, Dictionary<string, string> profileFields);
    Task<EnrollmentStatus?> VerifyEnrollmentStatusAsync(ProductMapping productMapping, UserMapping userMapping);
    Task<EnrollmentStatus?> VerifyEnrollmentStatusAsync(Product product, Customer customer, int? storeId = null);
}

public class NexportService : INexportService
{
    private readonly NexportApiService _nexportApi;
    private readonly Settings _settings;
    private readonly IUserMappingService _userMapping;
    private readonly IProductMappingService _productMappingService;
    private readonly ILocalizationService _localization;
    private readonly ISettingService _setting;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly INotificationService _notification;
    private readonly IStoreContext _storeContext;
    private readonly ITrainingPlanService _trainingPlan;
    private readonly ISectionService _section;
    private readonly HelperService _helper;

    #region Constructors

    public NexportService(
        NexportApiService apiService,
        Settings settings,
        ISettingService settingService,
        IGenericAttributeService genericAttributeService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IUserMappingService userMapping,
        IProductMappingService productMappingService,
        ITrainingPlanService trainingPlan,
        ISectionService section,
        HelperService helper)
    {
        _nexportApi = apiService;
        _settings = settings;
        _localization = localizationService;
        _setting = settingService;
        _genericAttribute = genericAttributeService;
        _notification = notificationService;
        _localization = localizationService;
        _storeContext = storeContext;
        _userMapping = userMapping;
        _productMappingService = productMappingService;
        _trainingPlan = trainingPlan;
        _section = section;
        _helper = helper;
    }

    #endregion Constructors

    public async Task GenerateNewTokenAsync(string url, string username, string password, bool tokenNeverExpired, DateTime? tokenExpirationDate)
    {
        DateTime? tokenExpiration = null;
        if (!tokenNeverExpired)
        {
            tokenExpiration = tokenExpirationDate ?? DateTime.UtcNow.AddDays(30);
        }
        var response = (await _nexportApi.AuthenticateApi(url, username, password, tokenExpiration)).Data;
        _settings.AuthenticationToken = response.AccessToken.ToString();
        _settings.Url = url;
        _settings.UtcExpirationDate = tokenExpiration;
        await _setting.SaveSettingAsync(_settings);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Plugins.Saved"));
    }

    public Task<List<DirectoryResponseItem>> SearchDirectory(string searchTerm, int? page = null)
        => _helper.Do(async s => (await _nexportApi.SearchDirectory((s.Url, s.Token), s.RootOrg, searchTerm, page)).Data, new List<DirectoryResponseItem>());

    public Task<SetCustomProfileFieldValuesResponse?> SetCustomProfileFieldValues(Guid subscriberId, Dictionary<string, string> profileFields)
        => _helper.Do(s => _nexportApi.SetCustomerProfileFieldValues((s.Url, s.Token), subscriberId, profileFields));

    public async Task<EnrollmentStatus?> VerifyEnrollmentStatusAsync(Product product, Customer customer, int? storeId = null)
    {
        if ((await _productMappingService.GetByNopProductId(product.Id, storeId) ?? await _productMappingService.GetByNopProductId(product.Id)) is not { } mapping)
        {
            return null;
        }
        else if (await _userMapping.FindByCustomerId(customer.Id) is { } userMapping)
        {
            return await VerifyEnrollmentStatusAsync(mapping, userMapping);
        }
        return null;
    }

    public async Task<EnrollmentStatus?> VerifyEnrollmentStatusAsync(ProductMapping productMapping, UserMapping userMapping)
    {
        if (!_settings.IsValid() || productMapping.SyllabusId is null)
        {
            return null;
        }
        var store = await _storeContext.GetCurrentStoreAsync();
        var orgId = productMapping.SubscriptionOrgId
            ?? await _genericAttribute.GetAttributeAsync<Guid?>(store, "SubscriptionOrganizationId", store.Id)
            ?? _settings.RootOrganizationId.Value;
        return productMapping.Type switch
        {
            Domain.ProductType.Section => await section(orgId, productMapping.SyllabusId.Value, userMapping),
            Domain.ProductType.TrainingPlan => await trainingPlan(orgId, productMapping.SyllabusId.Value, userMapping),
            _ => null
        };

        async ValueTask<EnrollmentStatus?> section(Guid orgId, Guid syllabusId, UserMapping userMapping)
            => await _section.GetSectionEnrollmentDetails(orgId, userMapping.UserId, syllabusId) is not SectionEnrollmentsResponse enrollment ? null
                : (enrollment.EnrollmentId, enrollment.Phase, enrollment.Result, enrollment.ExpirationDate, enrollment.PercentAssignmentsComplete);

        async ValueTask<EnrollmentStatus?> trainingPlan(Guid orgId, Guid syllabusId, UserMapping userMapping)
            => await _trainingPlan.GetTrainingPlanEnrollmentDetails(orgId, userMapping.UserId, syllabusId) is not TrainingPlanEnrollmentsResponse enrollment ? null
                : (enrollment.EnrollmentId, enrollment.Phase, enrollment.Result, enrollment.ExpirationDate, enrollment.PercentRequirementsFulfilled);
    }
}

public record struct EnrollmentStatus(Guid EnrollmentId, Enums.PhaseEnum Phase, Enums.ResultEnum Result, DateTime? EnrollmentExpirationDate, int CompletionPercentage)
{
    public static implicit operator (Guid EnrollmentId, Enums.PhaseEnum Phase, Enums.ResultEnum Result, DateTime? EnrollmentExpirationDate, int CompletionPercentage)(EnrollmentStatus value)
        => (value.EnrollmentId, value.Phase, value.Result, value.EnrollmentExpirationDate, value.CompletionPercentage);

    public static implicit operator EnrollmentStatus((Guid EnrollmentId, Enums.PhaseEnum Phase, Enums.ResultEnum Result, DateTime? EnrollmentExpirationDate, int CompletionPercentage) value)
        => new(value.EnrollmentId, value.Phase, value.Result, value.EnrollmentExpirationDate, value.CompletionPercentage);
}
