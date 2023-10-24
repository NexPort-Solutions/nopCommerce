using NexportApi.Api;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Models.Api;
using PublishingModel = NexportApi.Model.Enums.PublishingModelEnum;
using CatalogAccessOption = NexportApi.Model.CatalogRequest.CatalogAccessOptionEnum;
using ProductType = NexportApi.Model.Enums.ProductTypeEnum;
using PaymentProcessor = NexportApi.Model.InvoicePaymentRequest.PaymentProcessorEnum;
using RedemptionActionType = NexportApi.Model.RedeemInvoiceItemRequest.RedemptionActionTypeEnum;
using Nop.Plugin.Misc.Nexport.Services;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace Nop.Plugin.Misc.Nexport.Services;

public record Auth(string Url, string Token)
{
    public static implicit operator Auth((string, string) auth) => new(auth.Item1, auth.Item2);
}

public class NexportApiService
{

    private const string GRANT_TYPE_PASSWORD = "password";

    private readonly Configuration _apiConfiguration;

    private static ISynchronousClient Client => EngineContext.Current.Resolve<ISynchronousClient>();
    private static IAsynchronousClient AsynchronousClient => EngineContext.Current.Resolve<IAsynchronousClient>();
    private AdminApi AdminApi => new(Client, AsynchronousClient, _apiConfiguration);
    private LearningApi LearningApi => new(Client, AsynchronousClient, _apiConfiguration);
    private PointOfSaleApi PointOfSaleApi => new(Client, AsynchronousClient, _apiConfiguration);
    private SsoApi SsoApi => new(Client, AsynchronousClient, _apiConfiguration);

    public NexportApiService(Configuration apiConfiguration) => _apiConfiguration = apiConfiguration;

    public Task<Response<AuthenticationTokenResponse>> AuthenticateApi(string url, string username, string password, DateTime? tokenExp) => Response(new(url, null!), _
        => AdminApi.AdminApiAuthenticateWithHttpInfoAsync(new(username, password, GRANT_TYPE_PASSWORD, utcExpirationDate: tokenExp)));

    public Task<Response<GetUserResponse>> AuthenticateUser(Auth auth, string loginName, string password) => Response(auth, t
        => AdminApi.AdminApiAuthenticateUserWithHttpInfoAsync(loginName, password, t));

    public Task<Response<GetUserResponse>> GetUserByLogin(Auth auth, string loginName) => Response(auth, t
        => AdminApi.AdminApiGetUserWithHttpInfoAsync(t, loginName));

    public Task<Response<GetUserResponse>> GetUserByUserId(Auth auth, Guid userId) => Response(auth, t
        => AdminApi.AdminApiGetUserWithHttpInfoAsync(t, userId: userId));

    public Task<Response<UserContactInfoResponse>> GetUserContactInfo(Auth auth, Guid userId) => Response(auth, t
        => AdminApi.AdminApiGetUserContactInformationWithHttpInfoAsync(t, userId));

    public Task<Response<List<GetUserResponse>>> GetUsers(Auth auth, string searchTerm, int? page = null) => Response(auth, t
        => AdminApi.AdminApiGetUsersWithHttpInfoAsync(t, searchTerm, page));

    public Task<Response<CreateUserResponse>> CreateUser(Auth auth, (string, string) credentials, (string, string) name, string email, Guid ownerOrg, UserContactInfoRequest? info) => Response(auth, t
        => AdminApi.AdminApiCreateUserWithHttpInfoAsync(t, new(ownerOrg, credentials.Item1, credentials.Item2, name.Item1, null, name.Item2, email, contactInfo: info)));

    public Task<Response<EditUserResponse>> EditUserContactInfo(Auth auth, Guid userId, UserContactInfoRequest updatedInfo) => Response(auth, t
        => AdminApi.AdminApiEditUserWithHttpInfoAsync(t, new(userId, contactInfo: updatedInfo)));

    public Task<Response<List<DirectoryResponseItem>>> SearchDirectory(Auth auth, Guid baseOrgId, string searchTerm, int? page = null) => Response(auth, t
        => AdminApi.AdminApiSearchDirectoryWithHttpInfoAsync(0, baseOrgId, t, searchTerm, searchTerm, page));

    public Task<Response<List<OrganizationResponseItem>>> GetOrganizations(Auth auth, Guid orgId, int? page = null) => Response(auth, t
        => AdminApi.AdminApiGetOrganizationsWithHttpInfoAsync(t, orgId, page: page));

    public Task<SubscriptionResponse?> GetSubscription(Auth auth, Guid userId, Guid orgId) => Helper(auth, t
        => AdminApi.AdminApiGetSubscriptionAsync(t, userId, orgId: orgId));

    public Task<Response<List<SubscriptionResponse>>> GetSubscriptions(Auth auth, Guid userId, int? page = null) => Response(auth, t
        => AdminApi.AdminApiGetSubscriptionsWithHttpInfoAsync(t, userId, page: page));

    public Task<Response<CatalogResponse>> GetCatalogs(Auth auth, Guid orgId, int? page = null) => Response(auth, t
        => LearningApi.LearningApiGetCatalogsWithHttpInfoAsync(t, new(orgId, PublishingModel.ForSaleInMarketPlace, CatalogAccessOption.Owned), page));

    public Task<CatalogResponseItem?> GetCatalogDetails(Auth auth, Guid catalogId) => Helper(auth, t
        => LearningApi.LearningApiGetCatalogAsync(catalogId, t));

    public Task<GetDescriptionResponse?> GetCatalogDescription(Auth auth, Guid catalogId) => Helper(auth, t
        => LearningApi.LearningApiGetCatalogDescriptionAsync(catalogId, t));

    public Task<GetCatalogCreditHoursResponse?> GetCatalogCreditHours(Auth auth, Guid catalogId) => Helper(auth, t
        => LearningApi.LearningApiGetCatalogCreditHoursAsync(catalogId, t));

    public Task<Response<List<GetSyllabiResponseItem>>> GetSyllabuses(Auth auth, Guid catalogId, int? page = null) => Response(auth, t
        => LearningApi.LearningApiGetCatalogSyllabiWithHttpInfoAsync(catalogId, t, page));

    public Task<SectionResponse?> GetSectionDetails(Auth auth, Guid sectionId) => Helper(auth, t
        => LearningApi.LearningApiGetSectionAsync(t, sectionId));

    public Task<GetDescriptionResponse?> GetSectionDescription(Auth auth, Guid sectionId) => Helper(auth, t
        => LearningApi.LearningApiGetSectionDescriptionAsync(sectionId, t));

    public Task<GetObjectivesResponse?> GetSectionObjectives(Auth auth, Guid sectionId) => Helper(auth, t
        => LearningApi.LearningApiGetSectionObjectivesAsync(sectionId, t));

    public Task<TrainingPlanResponse?> GetTrainingPlanDetails(Auth auth, Guid trainingPlanId) => Helper(auth, t
        => LearningApi.LearningApiGetTrainingPlanAsync(t, trainingPlanId));

    public Task<GetDescriptionResponse?> GetTrainingPlanDescription(Auth auth, Guid trainingPlanId) => Helper(auth, t
        => LearningApi.LearningApiGetTrainingPlanDescriptionAsync(trainingPlanId, t));

    public Task<Response<GetInvoiceResponse>> GetInvoice(Auth auth, Guid invoiceId) => Response(auth, t
        => PointOfSaleApi.PointOfSaleApiGetInvoiceWithHttpInfoAsync(invoiceId, t));

    public Task<BeginInvoiceTransactionResponse?> BeginInvoiceTransaction(Auth auth, Guid orgId, Guid purchasingAgentId) => Helper(auth, t
        => PointOfSaleApi.PointOfSaleApiBeginInvoiceTransactionAsync(t, new(purchasingAgentId, orgId)));

    public Task<AddInvoiceItemResponse?> AddInvoiceItem
        (Auth auth, Guid invoiceId, Guid productId, ProductType type, Guid organization, IList<Guid> memberships, decimal cost, string? note, DateTime? expiration, string? limit)
        => Helper(auth, t => PointOfSaleApi.PointOfSaleApiAddInvoiceItemAsync(t, new(invoiceId, productId, type, cost, note, expiration, limit, organization, null, memberships.ToList())));

    public Task<AddInvoiceScheduledPaymentResponse?> AddInvoiceScheduledPayment(Auth auth, Guid invoiceId, decimal amount, DateTime dueDate, string? note = null) => Helper(auth, t
        => PointOfSaleApi.PointOfSaleApiAddInvoiceScheduledPaymentAsync(t, new(invoiceId, amount, dueDate, note)));

    public Task<AddInvoicePaymentResponse?> AddInvoicePayment
        (Auth auth, Guid invoiceId, decimal amount, Guid merchantAccountId, Guid payeeId, PaymentProcessor processor, string transactionId, DateTime dueDate, Guid? paymentCollectorId, string? note)
        => Helper(auth, t => PointOfSaleApi.PointOfSaleApiAddInvoicePaymentAsync(t, new(invoiceId, paymentCollectorId ?? new Guid(), merchantAccountId, payeeId, processor, transactionId, amount, dueDate, note))); // TODO why new Guid?

    public Task<CommitInvoiceResponse?> CommitInvoiceTransaction(Auth auth, Guid invoiceId) => Helper(auth, t
        => PointOfSaleApi.PointOfSaleApiCommitInvoiceTransactionAsync(t, new(invoiceId)));

    public Task<InvoiceRedemptionResponse?> RedeemInvoice(Auth auth, Guid redeemingUserId, RedemptionActionType redemptionAction, string? invoiceItemRedemptionCode) => Helper(auth, t
        => PointOfSaleApi.PointOfSaleApiRedeemInvoiceItemAsync(t, new(default, default, invoiceItemRedemptionCode, redeemingUserId, redemptionAction)));

    public Task<Response<InvoiceRedemptionResponse>> GetInvoiceRedemption(Auth auth, Guid invoiceItemId) => Response(auth, t
        => PointOfSaleApi.PointOfSaleApiGetInvoiceRedemptionWithHttpInfoAsync(invoiceItemId, t));

    public Task<SsoResponse?> SingleSignOn(Auth auth, Guid orgId, Guid userId, string redirectUrl) => Helper(auth, t
        => SsoApi.SsoApiSignInAsync(t, new(Enums.DisplayEnum.Normal, userId, orgId, redirectEntityRequest: new(redirectUrl, redirectUrl, redirectUrl))));

    public Task<SsoResponse?> ClassroomSingleSignOn(Auth auth, Guid enrollmentId, string redirectUrl) => Helper(auth, t
        => SsoApi.SsoApiClassroomAsync(t, new(Enums.DisplayEnum.Normal, enrollmentId, new(redirectUrl, redirectUrl, redirectUrl))));

    public Task<SectionEnrollmentsResponse?> GetSectionEnrollment(Auth auth, Guid orgId, Guid userId, Guid syllabusId)
        => Helper(auth, async t => (await LearningApi.LearningApiGetSectionEnrollmentsAsync(t, orgId, null, userId, syllabusId)).FirstOrDefault());

    public Task<TrainingPlanEnrollmentsResponse?> GetTrainingPlanEnrollment(Auth auth, Guid orgId, Guid userId, Guid syllabusId)
        => Helper(auth, async t => (await LearningApi.LearningApiGetTrainingPlanEnrollmentsAsync(t, orgId, userId: userId, syllabusId: syllabusId)).FirstOrDefault());

    public Task<CreateMembershipResponse?> CreateMemberships(Auth auth, Guid userId, IList<Guid> groupIds) => Helper(auth, t
        => AdminApi.AdminApiCreateMembershipAsync(t, new(groupIds.ToList(), userId)));

    public Task<RemoveMembershipResponse?> RemoveMemberships(Auth auth, IList<Guid> membershipIds) => Helper(auth, t
        => AdminApi.AdminApiRemoveMembershipAsync(t, new(memberships: membershipIds.ToList())));

    public Task<SetCustomProfileFieldValuesResponse?> SetCustomerProfileFieldValues(Auth auth, Guid subscriberId, Dictionary<string, string> profileFields) => Helper(auth, t
        => AdminApi.AdminApiSetCustomProfileFieldValuesAsync(t, new(subscriberId, customProfileFieldValues: profileFields)));

    private Task<Response<TResponse>> Response<TResponse>(Auth auth, Func<string, Task<ApiResponse<TResponse>>> f) => Helper(auth, t => f(t).ToResponse());

    private Task<TResponse> Helper<TResponse>(Auth auth, Func<string, Task<TResponse>> f)
    {
        _apiConfiguration.BasePath = auth.Url;
        return f(auth.Token);
    }
}
