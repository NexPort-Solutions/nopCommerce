using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NexportApi.Api;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models;
using SsoApiApi = NexportApi.Api.SsoApiApi;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportApiService
    {
        private readonly Configuration _apiConfiguration;

        public NexportApiService(Configuration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
        }

        public NexportAuthenticationResponseDetails AuthenticateNexportApi([NotNull]string url, [NotNull]string username, [NotNull]string password, DateTime? tokenExp)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(username))
                throw new NullReferenceException("User name cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new NullReferenceException("Password cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiAuthenticateWithHttpInfo(new AuthenticationTokenRequest(username, password,
                "password", utcExpirationDate: tokenExp));

            var result = new NexportAuthenticationResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails AuthenticateNexportUser([NotNull]string url, [NotNull]string accessToken, [NotNull]string loginName, [NotNull]string password)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            if (string.IsNullOrWhiteSpace(loginName))
                throw new NullReferenceException("Login name cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new NullReferenceException("Password cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiAuthenticateUserWithHttpInfo(loginName, password, accessToken);

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByLogin([NotNull]string url, [NotNull]string accessToken, [NotNull]string loginName)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            if (string.IsNullOrWhiteSpace(loginName))
                throw new NullReferenceException("Login name cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, loginName);

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByUserId([NotNull]string url, [NotNull]string accessToken, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, userId: userId.ToString());

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportUserListResponse GetNexportUsers([NotNull]string url, [NotNull]string accessToken, string searchTerm, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiGetUsersWithHttpInfo(accessToken, searchTerm, page);

            var result = new NexportUserListResponse
            {
                UserList = response.Data,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public NexportCreateUserResponseDetails CreateNexportUser([NotNull]string url, [NotNull]string accessToken,
            [NotNull]string login, [NotNull]string password,
            [NotNull]string firstName, [NotNull]string lastName, [NotNull]string email, Guid ownerOrgId)
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

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(url);
            var response = nexportApi.AdminApiCreateUserWithHttpInfo(
                new CreateUserRequest(ownerOrgId.ToString(), login, password, firstName, "", lastName, email), accessToken);

            var result = new NexportCreateUserResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportDirectoryResponse SearchNexportDirectory([NotNull]string url, [NotNull]string accessToken,
            Guid baseOrgId, string searchTerm, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiSearchDirectoryWithHttpInfo(0, baseOrgId.ToString(), accessToken, searchTerm, searchTerm, page);

            var result = new NexportDirectoryResponse
            {
                DirectoryList = response.Data,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public NexportOrganizationResponse GetNexportOrganizations([NotNull]string url, [NotNull]string accessToken, Guid rootOrgId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var response = nexportApi.LearningApiGetOrganizationsWithHttpInfo(accessToken, rootOrgId.ToString(), page: page);

            var result = new NexportOrganizationResponse
            {
                OrganizationList = response.Data,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public NexportCatalogResponse GetNexportCatalogs([NotNull]string url, [NotNull]string accessToken, Guid orgId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var catalogRequest = new CatalogRequest(orgId.ToString(), CatalogRequest.PublishingModelEnum.ForSaleInMarketPlace, CatalogRequest.CatalogAccessOptionEnum.Owned);
            var response = nexportApi.LearningApiGetCatalogsWithHttpInfo(catalogRequest, accessToken, page);

            var result = new NexportCatalogResponse
            {
                CatalogList = response.Data.Catalogs,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public CatalogResponseItem GetNexportCatalogDetails([NotNull]string url, [NotNull]string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalog(catalogId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportCatalogDescription([NotNull]string url, [NotNull]string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalogDescription(catalogId.ToString(), accessToken);

            return result;
        }

        public GetCatalogCreditHoursResponse GetNexportCatalogCreditHours([NotNull]string url, [NotNull]string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalogCreditHours(catalogId.ToString(), accessToken);

            return result;
        }

        public NexportSyllabusResponse GetNexportSyllabuses([NotNull]string url, [NotNull]string accessToken, Guid catalogId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var response = nexportApi.LearningApiGetCatalogSyllabiWithHttpInfo(catalogId.ToString(), accessToken, page);

            var result = new NexportSyllabusResponse
            {
                SyllabusList = response.Data,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public SectionResponse GetNexportSectionDetails([NotNull]string url, [NotNull]string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSection(sectionId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportSectionDescription([NotNull]string url, [NotNull]string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSectionDescription(sectionId.ToString(), accessToken);

            return result;
        }

        public GetObjectivesResponse GetNexportSectionObjectives([NotNull]string url, [NotNull]string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSectionObjectives(sectionId.ToString(), accessToken);

            return result;
        }

        public TrainingPlanResponse GetNexportTrainingPlanDetails([NotNull]string url, [NotNull]string accessToken, Guid trainingPlanId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetTrainingPlan(trainingPlanId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportTrainingPlanDescription([NotNull]string url, [NotNull]string accessToken, Guid trainingPlanId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetTrainingPlanDescription(trainingPlanId.ToString(), accessToken);

            return result;
        }

        public NexportGetInvoiceResponseDetails GetNexportInvoice([NotNull]string url, [NotNull]string accessToken, Guid invoiceId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var response = nexportApi.PointOfSaleApiGetInvoiceWithHttpInfo(invoiceId.ToString(), accessToken);

            var result = new NexportGetInvoiceResponseDetails
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public BeginInvoiceTransactionResponse BeginNexportInvoiceTransaction([NotNull]string url, [NotNull]string accessToken,
            Guid orgId, Guid purchasingAgentId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result = nexportApi.PointOfSaleApiBeginInvoiceTransaction(
                new CreateInvoiceMessageRequest(purchasingAgentId.ToString(), orgId.ToString()), accessToken);

            return result;
        }

        public AddInvoiceItemResponse AddNexportInvoiceItem([NotNull]string url, [NotNull]string accessToken,
            Guid invoiceId, Guid productId, Enums.ProductTypeEnum productType,
            Guid subscriptionOrgId, List<Guid> groupMembershipIds,
            decimal cost, string note = null, DateTime? accessExpirationDate = null,
            string accessExpirationTimeLimit = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            var groupMembershipIdList = groupMembershipIds.ConvertAll(x => x.ToString());

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiAddInvoiceItem(new CreateInvoiceItemRequest(invoiceId.ToString(), productId.ToString())
                {
                    ProductType = productType,
                    SubscriptionOrgId = subscriptionOrgId.ToString(),
                    GroupMembershipIds = groupMembershipIdList,
                    Note = note,
                    Cost = cost,
                    UtcAccessExpirationDate = accessExpirationDate,
                    AccessExpirationTimeLimit = accessExpirationTimeLimit
                }, accessToken);

            return result;
        }

        public AddInvoiceScheduledPaymentResponse AddNexportInvoiceScheduledPayment([NotNull]string url, [NotNull]string accessToken,
            Guid invoiceId, decimal amount, string note = null, DateTime? dueDate = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiAddInvoiceScheduledPayment(new InvoiceScheduledPaymentRequest(invoiceId.ToString(), amount, dueDate, note), accessToken);

            return result;
        }

        public AddInvoicePaymentResponse AddNexportInvoicePayment([NotNull]string url, [NotNull]string accessToken,
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

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiAddInvoicePayment(new InvoicePaymentRequest(
                    invoiceId.ToString(),
                    merchantAccountId: merchantAccountId.ToString(),
                    payeeId: payeeId.ToString(),
                    paymentProcessor: paymentProcessor,
                    paymentProcessorTransactionId: paymentProcessorTransactionId,
                    amountUsd: amount,
                    utcPaymentDate: dueDate)
                {
                    PaymentCollectorId = paymentCollectorId.HasValue ? paymentCollectorId.ToString() : null,
                    Note = note
                }, accessToken);

            return result;
        }

        public CommitInvoiceResponse CommitNexportInvoiceTransaction([NotNull]string url, [NotNull]string accessToken, Guid invoiceId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiCommitInvoiceTransaction(new CommitInvoiceRequest(invoiceId.ToString()), accessToken);

            return result;
        }

        public InvoiceRedemptionResponse RedeemNexportInvoice([NotNull]string url, [NotNull]string accessToken, Guid invoiceItemId,
            Guid redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiRedeemInvoiceItem(
                    new RedeemInvoiceItemRequest(invoiceItemId.ToString(), redeemingUserId: redeemingUserId.ToString(), redemptionActionType: redemptionAction), accessToken);

            return result;
        }

        public InvoiceRedemptionResponse GetNexportInvoiceRedemption([NotNull]string url, [NotNull]string accessToken, Guid invoiceItemId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiGetInvoiceRedemption(invoiceItemId.ToString(), accessToken);

            return result;
        }

        public SsoResponse NexportSingleSignOn([NotNull]string url, [NotNull]string accessToken, Guid orgId, Guid userId, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApiApi(_apiConfiguration);
            var result =
                nexportApi.SsoApiSignIn(new SsoRequest(
                    SsoRequest.DisplayEnum.Normal, userId.ToString(), orgId.ToString(),
                    redirectEntityRequest: new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)),
                    accessToken);

            return result;
        }

        public SsoResponse NexportClassroomSingleSignOn([NotNull]string url, [NotNull]string accessToken, [NotNull]string enrollmentId, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            if (string.IsNullOrWhiteSpace(enrollmentId))
                throw new NullReferenceException("Enrollment Id cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApiApi(_apiConfiguration);

            var result =
                nexportApi.SsoApiClassroom(new ClassroomSsoRequest(
                    ClassroomSsoRequest.DisplayEnum.Normal, enrollmentId,
                    new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)),
                    accessToken);

            return result;
        }
    }
}
