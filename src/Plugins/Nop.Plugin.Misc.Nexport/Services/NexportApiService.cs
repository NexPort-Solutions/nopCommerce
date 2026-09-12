using JetBrains.Annotations;
using NexportApi.Api;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Models.Api;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Enrollment;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Plugin.Misc.Nexport.Models.Subscription;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using ApiConfiguration = NexportApi.Client.Configuration;

namespace Nop.Plugin.Misc.Nexport.Services;

public class NexportApiService(ApiConfiguration apiConfiguration, IHttpClientFactory httpClientFactory)
{
    private AdminApi CreateAdminApi(string url)
    {
        var configuration = new ApiConfiguration { BasePath = url };
        var client = new ApiClient(httpClientFactory.CreateClient("NexportApi"), url);
        return new AdminApi(client, client, configuration);
    }

    private LearningApi CreateLearningApi(string url)
    {
        var configuration = new ApiConfiguration { BasePath = url };
        var client = new ApiClient(httpClientFactory.CreateClient("NexportApi"), url);
        return new LearningApi(client, client, configuration);
    }

    private PointOfSaleApi CreatePointOfSaleApi(string url)
    {
        var configuration = new ApiConfiguration { BasePath = url };
        var client = new ApiClient(httpClientFactory.CreateClient("NexportApi"), url);
        return new PointOfSaleApi(client, client, configuration);
    }

    private SsoApi CreateSsoApi(string url)
    {
        var configuration = new ApiConfiguration { BasePath = url };
        var client = new ApiClient(httpClientFactory.CreateClient("NexportApi"), url);
        return new SsoApi(client, client, configuration);
    }

    public async Task<NexportAuthenticationResponseDetails> AuthenticateNexportApiAsync([NotNull] string url,
        [NotNull] string username, [NotNull] string password, DateTime? tokenExp,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(username))
            throw new NullReferenceException("User name cannot be empty");

        if (string.IsNullOrWhiteSpace(password))
            throw new NullReferenceException("Password cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiAuthenticateWithHttpInfoAsync(new AuthenticationTokenRequest(username, password,
            "password", utcExpirationDate: tokenExp), cancellationToken);

        var result = new NexportAuthenticationResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportGetUserResponseDetails> AuthenticateNexportUserAsync([NotNull] string url,
        [NotNull] string accessToken, [NotNull] string loginName, [NotNull] string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        if (string.IsNullOrWhiteSpace(loginName))
            throw new NullReferenceException("Login name cannot be empty");

        if (string.IsNullOrWhiteSpace(password))
            throw new NullReferenceException("Password cannot be empty");

        var nexportApi = CreateAdminApi(url);

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(30));
        var response = await nexportApi.AdminApiAuthenticateUserWithHttpInfoAsync(
            loginName,
            password,
            accessToken,
            timeoutSource.Token);

        var result = new NexportGetUserResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportGetUserResponseDetails> GetNexportUserByLoginAsync([NotNull] string url,
        [NotNull] string accessToken, [NotNull] string loginName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        if (string.IsNullOrWhiteSpace(loginName))
            throw new NullReferenceException("Login name cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetUserWithHttpInfoAsync(accessToken, loginName,
            cancellationToken: cancellationToken);

        var result = new NexportGetUserResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportGetUserResponseDetails> GetNexportUserByUserIdAsync([NotNull] string url, [NotNull] string accessToken,
        Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetUserWithHttpInfoAsync(accessToken, userId: userId,
            cancellationToken: cancellationToken);

        var result = new NexportGetUserResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportUserContactInfoResponseDetails> GetNexportUserContactInfoAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetUserContactInformationWithHttpInfoAsync(accessToken, userId,
            cancellationToken: cancellationToken);

        var result = new NexportUserContactInfoResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportUserListResponse> GetNexportUsersAsync([NotNull] string url, [NotNull] string accessToken,
        string searchTerm, int? page = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetUsersWithHttpInfoAsync(accessToken, searchTerm, page, perPage: 30,
            cancellationToken: cancellationToken);

        var result = new NexportUserListResponse
        {
            UserList = response.Data
        };

        if (response.Headers.ContainsKey("X-Total-Count"))
            result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Per-Page"))
            result.RecordPerPage = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Page"))
            result.CurrentPage = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<NexportCreateUserResponseDetails> CreateNexportUserAsync([NotNull] string url,
        [NotNull] string accessToken,
        [NotNull] string login, [NotNull] string password,
        [NotNull] string firstName, [NotNull] string lastName, [NotNull] string email, Guid ownerOrgId,
        UserContactInfoRequest contactInfo = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        if (string.IsNullOrWhiteSpace(login))
            throw new NullReferenceException("Login cannot be empty");

        if (string.IsNullOrWhiteSpace(password))
            throw new NullReferenceException("Password cannot be empty");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new NullReferenceException("First name cannot be empty");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new NullReferenceException("Last name cannot be empty");

        if (string.IsNullOrWhiteSpace(email))
            throw new NullReferenceException("Email cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiCreateUserWithHttpInfoAsync(accessToken,
            new CreateUserRequest(ownerOrgId, login, password, firstName, "", lastName, email, contactInfo: contactInfo),
            cancellationToken);

        var result = new NexportCreateUserResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportEditUserResponseDetails> EditNexportUserContactInfoAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, UserContactInfoRequest updatedInfo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        if (updatedInfo == null)
            throw new NullReferenceException("Updated information cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiEditUserWithHttpInfoAsync(accessToken,
            new EditUserRequest(userId, contactInfo: updatedInfo), cancellationToken);

        var result = new NexportEditUserResponseDetails()
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<NexportDirectoryResponse> SearchNexportDirectoryAsync([NotNull] string url,
        [NotNull] string accessToken, Guid baseOrgId, string searchTerm, int? page = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiSearchDirectoryWithHttpInfoAsync(0, baseOrgId, accessToken, searchTerm,
            searchTerm, page, perPage: 30, cancellationToken: cancellationToken);

        var result = new NexportDirectoryResponse
        {
            DirectoryList = response.Data
        };

        if (response.Headers.ContainsKey("X-Total-Count"))
            result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Per-Page"))
            result.RecordPerPage = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Page"))
            result.CurrentPage = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<NexportOrganizationResponse> GetNexportOrganizationsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, int? page = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetOrganizationsWithHttpInfoAsync(accessToken, orgId, page: page,
            perPage: 30, cancellationToken: cancellationToken);

        var result = new NexportOrganizationResponse
        {
            OrganizationList = response.Data
        };

        if (response.Headers.ContainsKey("X-Total-Count"))
            result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Per-Page"))
            result.RecordPerPage = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Page"))
            result.CurrentPage = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<SubscriptionResponse> GetNexportSubscriptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, Guid orgId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var result = await nexportApi.AdminApiGetSubscriptionAsync(accessToken, userId, orgId: orgId,
            cancellationToken: cancellationToken);

        return result;
    }

    public async Task<NexportSubscriptionsResponse> GetNexportSubscriptionsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, int? page = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiGetSubscriptionsWithHttpInfoAsync(accessToken, userId: userId,
            page: page, perPage: 30);

        var result = new NexportSubscriptionsResponse
        {
            Subscriptions = response.Data
        };

        if (response.Headers.TryGetValue("X-Total-Count", out var totalCounteHeader))
            result.TotalRecord = int.Parse(totalCounteHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Per-Page", out var perPageHeader))
            result.RecordPerPage = int.Parse(perPageHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Page", out var pageHeader))
            result.CurrentPage = int.Parse(pageHeader.FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<NexportEnrollmentsResponse> GetNexportEnrollmentsAsync([NotNull] string url,
        [NotNull] string accessToken, EnrollmentSearchFilter searchFilter, int? page = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var response = await nexportApi.LearningApiSearchEnrollmentsWithHttpInfoAsync(accessToken, searchFilter, page,
            perPage: 30);

        var result = new NexportEnrollmentsResponse();
        if (response.Data != null)
        {
            result.EnrollmentList = response.Data.Enrollments;
        }

        if (response.Headers.TryGetValue("X-Total-Count", out var totalCounteHeader))
            result.TotalRecord = int.Parse(totalCounteHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Per-Page", out var perPageHeader))
            result.RecordPerPage = int.Parse(perPageHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Page", out var pageHeader))
            result.CurrentPage = int.Parse(pageHeader.FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<NexportCatalogResponse> GetNexportCatalogsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, int? page = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var catalogRequest = new CatalogRequest(orgId, Enums.PublishingModelEnum.ForSaleInMarketPlace, CatalogRequest.CatalogAccessOptionEnum.Owned);
        var response = await nexportApi.LearningApiGetCatalogsWithHttpInfoAsync(accessToken, catalogRequest, page,
            perPage: 30, cancellationToken: cancellationToken);

        var result = new NexportCatalogResponse
        {
            CatalogList = response.Data.Catalogs
        };

        if (response.Headers.ContainsKey("X-Total-Count"))
            result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Per-Page"))
            result.RecordPerPage = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Page"))
            result.CurrentPage = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<CatalogResponseItem> GetNexportCatalogDetailsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid catalogId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetCatalogAsync(catalogId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<GetDescriptionResponse> GetNexportCatalogDescriptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid catalogId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetCatalogDescriptionAsync(catalogId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<GetCatalogCreditHoursResponse> GetNexportCatalogCreditHoursAsync([NotNull] string url,
        [NotNull] string accessToken, Guid catalogId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetCatalogCreditHoursAsync(catalogId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<NexportSyllabusResponse> GetNexportSyllabusesAsync([NotNull] string url,
        [NotNull] string accessToken, Guid catalogId, int? page = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var response = await nexportApi.LearningApiGetCatalogSyllabiWithHttpInfoAsync(catalogId, accessToken, page,
            perPage: 30, cancellationToken: cancellationToken);

        var result = new NexportSyllabusResponse
        {
            SyllabusList = response.Data
        };

        if (response.Headers.ContainsKey("X-Total-Count"))
            result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Per-Page"))
            result.RecordPerPage = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

        if (response.Headers.ContainsKey("X-Page"))
            result.CurrentPage = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<SectionResponse> GetNexportSectionDetailsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid sectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetSectionAsync(accessToken, sectionId,
            cancellationToken: cancellationToken);

        return result;
    }

    public async Task<GetDescriptionResponse> GetNexportSectionDescriptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid sectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetSectionDescriptionAsync(sectionId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<GetObjectivesResponse> GetNexportSectionObjectivesAsync([NotNull] string url,
        [NotNull] string accessToken, Guid sectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetSectionObjectivesAsync(sectionId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<TrainingPlanResponse> GetNexportTrainingPlanDetailsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid trainingPlanId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetTrainingPlanAsync(accessToken, trainingPlanId,
            cancellationToken: cancellationToken);

        return result;
    }

    public async Task<GetDescriptionResponse> GetNexportTrainingPlanDescriptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid trainingPlanId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiGetTrainingPlanDescriptionAsync(trainingPlanId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<DropDeleteEnrollmentResponse> DropNexportEnrollmentAsync([NotNull] string url,
        [NotNull] string accessToken, Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiDropEnrollmentAsync(enrollmentId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<DropDeleteEnrollmentResponse> DestroyNexportEnrollmentAsync([NotNull] string url,
        [NotNull] string accessToken, Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi.LearningApiDeleteEnrollmentAsync(enrollmentId, accessToken,
            cancellationToken);

        return result;
    }

    public async Task<NexportGetInvoiceResponseDetails> GetNexportInvoiceAsync([NotNull] string url,
        [NotNull] string accessToken, Guid invoiceId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var response = await nexportApi.PointOfSaleApiGetInvoiceWithHttpInfoAsync(invoiceId, accessToken);

        var result = new NexportGetInvoiceResponseDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<BeginInvoiceTransactionResponse> BeginNexportInvoiceTransactionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, Guid purchasingAgentId, Guid? purchasingGroupId = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiBeginInvoiceTransactionAsync(accessToken,
            new CreateInvoiceMessageRequest(purchasingAgentId, orgId, purchasingGroupId));

        return result;
    }

    public async Task<AddInvoiceItemResponse> AddNexportInvoiceItemAsync([NotNull] string url, [NotNull] string accessToken,
        Guid invoiceId, Guid productId, Enums.ProductTypeEnum productType,
        Guid subscriptionOrgId, IList<Guid> groupMembershipIds,
        decimal cost, string note = null, DateTime? accessExpirationDate = null, string accessExpirationTimeLimit = null,
        Guid? purchasingGroupId = null, string fundingPool = null, DateTime? redemptionAvailableDate = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiAddInvoiceItemAsync(accessToken,
            new CreateInvoiceItemRequest(invoiceId, productId)
            {
                ProductType = productType,
                SubscriptionOrgId = subscriptionOrgId,
                GroupMembershipIds = groupMembershipIds.ToList(),
                Note = note,
                Cost = cost,
                UtcAccessExpirationDate = accessExpirationDate,
                AccessExpirationTimeLimit = accessExpirationTimeLimit,
                PurchasingGroupId = purchasingGroupId,
                FundingPool = fundingPool,
                UtcRedemptionAvailableDate = redemptionAvailableDate
            });

        return result;
    }

    public async Task<AddInvoiceItemsResponse> AddNexportInvoiceItemsAsync([NotNull] string url, [NotNull] string accessToken,
        Guid invoiceId, Guid productId, Enums.ProductTypeEnum productType,
        Guid subscriptionOrgId, IList<Guid> groupMembershipIds,
        decimal cost, int quantity = 1, string note = null, DateTime? accessExpirationDate = null, string accessExpirationTimeLimit = null,
        Guid? purchasingGroupId = null, string fundingPool = null, DateTime? redemptionAvailableDate = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiAddInvoiceItemsAsync(accessToken,
            new CreateInvoiceItemsRequest
            {
                InvoiceId = invoiceId,
                ProductId = productId,
                Quantity = quantity,
                ProductType = productType,
                SubscriptionOrgId = subscriptionOrgId,
                GroupMembershipIds = groupMembershipIds.ToList(),
                Note = note,
                Cost = cost,
                UtcAccessExpirationDate = accessExpirationDate,
                AccessExpirationTimeLimit = accessExpirationTimeLimit,
                PurchasingGroupId = purchasingGroupId,
                FundingPool = fundingPool,
                UtcRedemptionAvailableDate = redemptionAvailableDate
            });

        return result;
    }

    public AddInvoiceScheduledPaymentResponse AddNexportInvoiceScheduledPayment([NotNull] string url, [NotNull] string accessToken,
        Guid invoiceId, decimal amount, DateTime dueDate, string note = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        apiConfiguration.BasePath = url;

        var nexportApi = new PointOfSaleApi(apiConfiguration)
        {
            Client = EngineContext.Current.Resolve<ISynchronousClient>(),
            AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
        };

        var result = nexportApi.PointOfSaleApiAddInvoiceScheduledPayment(accessToken, new InvoiceScheduledPaymentRequest(invoiceId, amount, dueDate, note));

        return result;
    }

    public async Task<AddInvoicePaymentResponse> AddNexportInvoicePaymentAsync([NotNull] string url,
        [NotNull] string accessToken,
        Guid invoiceId, decimal amount, Guid merchantAccountId,
        Guid payeeId, InvoicePaymentRequest.PaymentProcessorEnum paymentProcessor,
        string paymentProcessorTransactionId,
        DateTime dueDate,
        Guid? paymentCollectorId = null,
        string note = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiAddInvoicePaymentAsync(accessToken,
            new InvoicePaymentRequest(
                invoiceId,
                merchantAccountId: merchantAccountId,
                payeeId: payeeId,
                paymentProcessor: paymentProcessor,
                paymentProcessorTransactionId: paymentProcessorTransactionId,
                amountUsd: amount,
                utcPaymentDate: dueDate)
            {
                PaymentCollectorId = paymentCollectorId ?? Guid.NewGuid(),
                Note = note
            });

        return result;
    }

    public async Task<CommitInvoiceResponse> CommitNexportInvoiceTransactionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid invoiceId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiCommitInvoiceTransactionAsync(accessToken,
            new CommitInvoiceRequest(invoiceId));

        return result;
    }

    public async Task<InvoiceRedemptionResponse> RedeemNexportInvoiceAsync([NotNull] string url,
        [NotNull] string accessToken,
        Guid redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction, string invoiceItemRedemptionCode,
        Guid? productId = null, Enums.ProductTypeEnum? productType = null,
        UpdatedInvoiceFields updatedInvoiceFields = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        InvoiceRedemptionResponse result;
        if (productId != null)
        {
            result = await nexportApi.PointOfSaleApiRedeemInvoiceItemAsync(accessToken,
                new RedeemInvoiceItemRequest(invoiceItemRedemptionCode: invoiceItemRedemptionCode, redeemingUserId: redeemingUserId, redemptionActionType: redemptionAction,
                    productId: productId.Value, productType: productType, invoiceUpdates: updatedInvoiceFields));
        }
        else
        {
            result = await nexportApi.PointOfSaleApiRedeemInvoiceItemAsync(accessToken,
                new RedeemInvoiceItemRequest(invoiceItemRedemptionCode: invoiceItemRedemptionCode, redeemingUserId: redeemingUserId,
                    redemptionActionType: redemptionAction, invoiceUpdates: updatedInvoiceFields));
        }

        return result;
    }

    public InvoiceRedemptionResponse RedeemOpenEndedNexportInvoice([NotNull] string url, [NotNull] string accessToken,
        Guid redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction, string invoiceItemRedemptionCode, Guid? productId, Enums.ProductTypeEnum productType)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        if (!productId.HasValue)
            throw new NullReferenceException("Product Id cannot be null");

        apiConfiguration.BasePath = url;

        var nexportApi = new PointOfSaleApi(apiConfiguration)
        {
            Client = EngineContext.Current.Resolve<ISynchronousClient>(),
            AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
        };

        var result = nexportApi.PointOfSaleApiRedeemInvoiceItem(accessToken,
            new RedeemInvoiceItemRequest(invoiceItemRedemptionCode: invoiceItemRedemptionCode, redeemingUserId: redeemingUserId, redemptionActionType: redemptionAction,
                productId: productId.Value, productType: productType));

        return result;
    }

    public async Task<NexportGetInvoiceRedemptionDetails> GetNexportInvoiceRedemptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid invoiceItemId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var response = await nexportApi.PointOfSaleApiGetInvoiceRedemptionWithHttpInfoAsync(invoiceItemId, accessToken);

        var result = new NexportGetInvoiceRedemptionDetails
        {
            Response = response.Data,
            StatusCode = (int)response.StatusCode
        };

        return result;
    }

    public async Task<SsoResponse> NexportSingleSignOnAsync([NotNull] string url, [NotNull] string accessToken,
        Guid orgId, Guid userId, string redirectUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateSsoApi(url);

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(30));
        var result = await nexportApi.SsoApiSignInAsync(accessToken,
            new SsoRequest(Enums.DisplayEnum.Normal, userId, orgId,
                redirectEntityRequest: new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)),
            timeoutSource.Token);

        return result;
    }

    public async Task<SsoResponse> NexportClassroomSingleSignOnAsync([NotNull] string url,
        [NotNull] string accessToken, Guid enrollmentId, string redirectUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateSsoApi(url);

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(30));
        var result = await nexportApi.SsoApiClassroomAsync(accessToken,
            new ClassroomSsoRequest(Enums.DisplayEnum.Normal, enrollmentId,
                new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)),
            timeoutSource.Token);

        return result;
    }

    public async Task<NexportGetSectionEnrollmentsResponse> GetNexportSectionEnrollmentsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var response = await nexportApi.LearningApiGetSectionEnrollmentsWithHttpInfoAsync(accessToken, orgId, null, userId,
            cancellationToken: cancellationToken);

        var result = new NexportGetSectionEnrollmentsResponse();
        if (response != null)
        {
            result.SectionEnrollments = response.Data;

            if (response.Headers.TryGetValue("X-Total-Count", out var totalCountHeader))
                result.TotalRecord = int.Parse(totalCountHeader.FirstOrDefault() ?? "0");

            if (response.Headers.TryGetValue("X-Per-Page", out var perPageHeader))
                result.RecordPerPage = int.Parse(perPageHeader.FirstOrDefault() ?? "0");

            if (response.Headers.TryGetValue("X-Page", out var pageHeader))
                result.CurrentPage = int.Parse(pageHeader.FirstOrDefault() ?? "0");
        }

        return result;
    }

    public async Task<SectionEnrollmentsResponse> GetNexportSectionEnrollmentAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, Guid userId, Guid syllabusId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = (await nexportApi
                .LearningApiGetSectionEnrollmentsAsync(accessToken, orgId, null, userId, syllabusId))
            .FirstOrDefault();

        return result;
    }

    public List<TrainingPlanEnrollmentsResponse> GetNexportTrainingPlanEnrollments([NotNull] string url, [NotNull] string accessToken, Guid orgId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        apiConfiguration.BasePath = url;

        var nexportApi = new LearningApi(apiConfiguration)
        {
            Client = EngineContext.Current.Resolve<ISynchronousClient>(),
            AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
        };

        var result = nexportApi.LearningApiGetTrainingPlanEnrollments(accessToken, orgId, null, userId);

        return result;
    }

    public async Task<TrainingPlanEnrollmentsResponse> GetNexportTrainingPlanEnrollmentAsync([NotNull] string url,
        [NotNull] string accessToken, Guid orgId, Guid userId, Guid syllabusId)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = (await nexportApi
                .LearningApiGetTrainingPlanEnrollmentsAsync(accessToken, orgId, userId: userId, syllabusId: syllabusId))
            .FirstOrDefault();

        return result;
    }

    public async Task<CertificateUrlResponse> GetNexportEnrollmentCertificateUrlAsync([NotNull] string url,
        [NotNull] string accessToken, Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateLearningApi(url);

        var result = await nexportApi
            .LearningApiGetCertificateUrlAsync(enrollmentId, accessToken, cancellationToken);

        return result;
    }

    public async Task<CreateMembershipResponse> CreateNexportMembershipsAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, IList<Guid> groupIds)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var result = await nexportApi.AdminApiCreateMembershipAsync(accessToken,
            new CreateMembershipRequest(groupIds.ToList(), userId));

        return result;
    }

    public async Task<RemoveMembershipResponse> RemoveNexportMembershipsAsync([NotNull] string url,
        [NotNull] string accessToken, IList<Guid> membershipIds)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var result = await nexportApi.AdminApiRemoveMembershipAsync(accessToken,
            new RemoveMembershipRequest(memberships: membershipIds.ToList()));

        return result;
    }

    public async Task<SetCustomProfileFieldValuesResponse> SetNexportCustomerProfileFieldValuesAsync([NotNull] string url,
        [NotNull] string accessToken, Guid subscriberId, Dictionary<string, string> profileFields)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var result = await nexportApi.AdminApiSetCustomProfileFieldValuesAsync(accessToken,
            new SetCustomProfileFieldValuesRequest(subscriberId: subscriberId, customProfileFieldValues: profileFields));

        return result;
    }

    public async Task<HasGroupPermissionResponse> HasGroupPermissionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid userId, Guid groupId,
        string permission = NexportDefaults.NEXPORT_PURCHASING_AGENT_PERMISSION,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var result = await nexportApi.AdminApiHasGroupPermissionAsync(accessToken: accessToken,
            new HasGroupPermissionRequest(userId: userId, groupId: groupId, permission: permission),
            cancellationToken);

        return result;
    }

    public async Task<NexportSearchGroupsForPermissionResponse> SearchGroupsForPermissionAsync([NotNull] string url,
        [NotNull] string accessToken,
        Guid userId, Guid groupId,
        string permission = NexportDefaults.NEXPORT_PURCHASING_AGENT_PERMISSION, int? page = null, int perPage = 30,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreateAdminApi(url);

        var response = await nexportApi.AdminApiSearchGroupsForPermissionWithHttpInfoAsync(accessToken,
            new SearchGroupsForPermissionRequest(userId: userId, orgId: groupId, permission: permission, page: page, perPage: perPage),
            cancellationToken);

        var result = new NexportSearchGroupsForPermissionResponse
        {
            SearchGroupsForPermissionList = response.Data != null ? response.Data.Groups : []
        };

        if (response.Headers.TryGetValue("X-Total-Count", out var totalCountHeader))
            result.TotalRecord = int.Parse(totalCountHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Per-Page", out var perPageHeader))
            result.RecordPerPage = int.Parse(perPageHeader.FirstOrDefault() ?? "0");

        if (response.Headers.TryGetValue("X-Page", out var pageHeader))
            result.CurrentPage = int.Parse(pageHeader.FirstOrDefault() ?? "0");

        return result;
    }

    public async Task<ResetInvoiceRedemptionResponse> ResetInvoiceRedemptionAsync([NotNull] string url,
        [NotNull] string accessToken, Guid invoiceItemId, Guid? resetAdminId = null, string note = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        note = string.IsNullOrWhiteSpace(note) ? $"Reset redemption for invoice item with id: {invoiceItemId} on {DateTime.UtcNow}" : note;

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiResetInvoiceRedemptionAsync(accessToken,
            new ResetInvoiceRedemptionRequest(invoiceItemId, note, NexportDefaults.REMOTE_SYS_NAME_FOR_API));

        return result;
    }

    //public UpdateInvoiceItemResponse UpdateInvoiceItem([NotNull] string url, [NotNull] string accessToken,
    //    Guid invoiceItemId, Guid? productId = null, Enums.ProductTypeEnum productType = Enums.ProductTypeEnum.Syllabus,
    //    string productCode = null, bool isRenewal = false,
    //    decimal? cost = null, string note = null,
    //    DateTime? accessExpirationDate = null, string accessExpirationTimeLimit = null,
    //    Guid? subscriptionOrgId = null, IList<Guid> groupMembershipIds = null,
    //    Guid? purchasingGroupId = null, string fundingPool = null, DateTime? redemptionAvailableDate = null)
    public async Task<UpdateInvoiceItemResponse> UpdateInvoiceItemAsync([NotNull] string url,
        [NotNull] string accessToken, Guid invoiceItemId, UpdatedInvoiceFields updatedInvoiceFields,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new NullReferenceException("Api url cannot be empty");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new NullReferenceException("Access token cannot be empty");

        var nexportApi = CreatePointOfSaleApi(url);

        var result = await nexportApi.PointOfSaleApiUpdateInvoiceItemAsync(accessToken,
            new UpdateInvoiceItemRequest(invoiceItemId, updatedInvoiceFields), cancellationToken);

        return result;
    }
}