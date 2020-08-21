using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NexportApi.Api;
using NexportApi.Client;
using NexportApi.Model;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Models.Api;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Plugin.Misc.Nexport.Models.Subscription;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportApiService
    {
        private readonly Configuration _apiConfiguration;

        public NexportApiService(Configuration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
        }

        public NexportAuthenticationResponseDetails AuthenticateNexportApi([NotNull] string url, [NotNull] string username, [NotNull] string password, DateTime? tokenExp)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(username))
                throw new NullReferenceException("User name cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new NullReferenceException("Password cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiAuthenticateWithHttpInfo(new AuthenticationTokenRequest(username, password,
                "password", utcExpirationDate: tokenExp));

            var result = new NexportAuthenticationResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails AuthenticateNexportUser([NotNull] string url, [NotNull] string accessToken, [NotNull] string loginName, [NotNull] string password)
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

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiAuthenticateUserWithHttpInfo(loginName, password, accessToken);

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByLogin([NotNull] string url, [NotNull] string accessToken, [NotNull] string loginName)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            if (string.IsNullOrWhiteSpace(loginName))
                throw new NullReferenceException("Login name cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, loginName);

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByUserId([NotNull] string url, [NotNull] string accessToken, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, userId: userId);

            var result = new NexportGetUserResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportUserListResponse GetNexportUsers([NotNull] string url, [NotNull] string accessToken, string searchTerm, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiGetUsersWithHttpInfo(accessToken, searchTerm, page);

            var result = new NexportUserListResponse
            {
                UserList = response.Data
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public NexportCreateUserResponseDetails CreateNexportUser([NotNull] string url, [NotNull] string accessToken,
            [NotNull] string login, [NotNull] string password,
            [NotNull] string firstName, [NotNull] string lastName, [NotNull] string email, Guid ownerOrgId)
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

            var nexportApi = new AdminApi(url)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiCreateUserWithHttpInfo(accessToken,
                new CreateUserRequest(ownerOrgId, login, password, firstName, "", lastName, email));

            var result = new NexportCreateUserResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportEditUserResponseDetails EditNexportUserContactInfo([NotNull] string url, [NotNull] string accessToken,
            Guid userId, UserContactInfoRequest updatedInfo)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            if (updatedInfo == null)
                throw new NullReferenceException("Updated information cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(url)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response =
                nexportApi.AdminApiEditUserWithHttpInfo(accessToken, new EditUserRequest(userId, contactInfo: updatedInfo));

            var result = new NexportEditUserResponseDetails()
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public NexportDirectoryResponse SearchNexportDirectory([NotNull] string url, [NotNull] string accessToken,
            Guid baseOrgId, string searchTerm, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiSearchDirectoryWithHttpInfo(0, baseOrgId, accessToken, searchTerm, searchTerm, page);

            var result = new NexportDirectoryResponse
            {
                DirectoryList = response.Data
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public NexportOrganizationResponse GetNexportOrganizations([NotNull] string url, [NotNull] string accessToken, Guid orgId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiGetOrganizationsWithHttpInfo(accessToken, orgId, page: page);

            var result = new NexportOrganizationResponse
            {
                OrganizationList = response.Data
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public NexportSubscriptionsResponse GetNexportSubscriptions([NotNull] string url, [NotNull] string accessToken, Guid userId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.AdminApiGetSubscriptionsWithHttpInfo(accessToken, userId, page: page);

            var result = new NexportSubscriptionsResponse
            {
                Subscriptions = response.Data
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public NexportCatalogResponse GetNexportCatalogs([NotNull] string url, [NotNull] string accessToken, Guid orgId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var catalogRequest = new CatalogRequest(orgId, CatalogRequest.PublishingModelEnum.ForSaleInMarketPlace, CatalogRequest.CatalogAccessOptionEnum.Owned);
            var response = nexportApi.LearningApiGetCatalogsWithHttpInfo(accessToken, catalogRequest, page);

            var result = new NexportCatalogResponse
            {
                CatalogList = response.Data.Catalogs
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public CatalogResponseItem GetNexportCatalogDetails([NotNull] string url, [NotNull] string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetCatalog(catalogId, accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportCatalogDescription([NotNull] string url, [NotNull] string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetCatalogDescription(catalogId, accessToken);

            return result;
        }

        public GetCatalogCreditHoursResponse GetNexportCatalogCreditHours([NotNull] string url, [NotNull] string accessToken, Guid catalogId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetCatalogCreditHours(catalogId, accessToken);

            return result;
        }

        public NexportSyllabusResponse GetNexportSyllabuses([NotNull] string url, [NotNull] string accessToken, Guid catalogId, int? page = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.LearningApiGetCatalogSyllabiWithHttpInfo(catalogId, accessToken, page);

            var result = new NexportSyllabusResponse
            {
                SyllabusList = response.Data
            };

            if (response.Headers.ContainsKey("X-Total-Count"))
                result.TotalRecord = int.Parse(response.Headers["X-Total-Count"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Per-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Per-Page"].FirstOrDefault() ?? "0");

            if (response.Headers.ContainsKey("X-Page"))
                result.TotalRecord = int.Parse(response.Headers["X-Page"].FirstOrDefault() ?? "0");

            return result;
        }

        public SectionResponse GetNexportSectionDetails([NotNull] string url, [NotNull] string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetSection(accessToken, sectionId);

            return result;
        }

        public GetDescriptionResponse GetNexportSectionDescription([NotNull] string url, [NotNull] string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetSectionDescription(sectionId, accessToken);

            return result;
        }

        public GetObjectivesResponse GetNexportSectionObjectives([NotNull] string url, [NotNull] string accessToken, Guid sectionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetSectionObjectives(sectionId, accessToken);

            return result;
        }

        public TrainingPlanResponse GetNexportTrainingPlanDetails([NotNull] string url, [NotNull] string accessToken, Guid trainingPlanId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetTrainingPlan(accessToken, trainingPlanId);

            return result;
        }

        public GetDescriptionResponse GetNexportTrainingPlanDescription([NotNull] string url, [NotNull] string accessToken, Guid trainingPlanId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetTrainingPlanDescription(trainingPlanId, accessToken);

            return result;
        }

        public NexportGetInvoiceResponseDetails GetNexportInvoice([NotNull] string url, [NotNull] string accessToken, Guid invoiceId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var response = nexportApi.PointOfSaleApiGetInvoiceWithHttpInfo(invoiceId, accessToken);

            var result = new NexportGetInvoiceResponseDetails
            {
                Response = response.Data,
                StatusCode = (int)response.StatusCode
            };

            return result;
        }

        public BeginInvoiceTransactionResponse BeginNexportInvoiceTransaction([NotNull] string url, [NotNull] string accessToken,
            Guid orgId, Guid purchasingAgentId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiBeginInvoiceTransaction(accessToken,
                new CreateInvoiceMessageRequest(purchasingAgentId, orgId));

            return result;
        }

        public AddInvoiceItemResponse AddNexportInvoiceItem([NotNull] string url, [NotNull] string accessToken,
            Guid invoiceId, Guid productId, Enums.ProductTypeEnum productType,
            Guid subscriptionOrgId, IList<Guid> groupMembershipIds,
            decimal cost, string note = null, DateTime? accessExpirationDate = null,
            string accessExpirationTimeLimit = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiAddInvoiceItem(accessToken,
                new CreateInvoiceItemRequest(invoiceId, productId)
                {
                    ProductType = productType,
                    SubscriptionOrgId = subscriptionOrgId,
                    GroupMembershipIds = groupMembershipIds.ToList(),
                    Note = note,
                    Cost = cost,
                    UtcAccessExpirationDate = accessExpirationDate,
                    AccessExpirationTimeLimit = accessExpirationTimeLimit
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

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiAddInvoiceScheduledPayment(accessToken, new InvoiceScheduledPaymentRequest(invoiceId, amount, dueDate, note));

            return result;
        }

        public AddInvoicePaymentResponse AddNexportInvoicePayment([NotNull] string url, [NotNull] string accessToken,
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

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiAddInvoicePayment(accessToken,
                new InvoicePaymentRequest(
                    invoiceId,
                    merchantAccountId: merchantAccountId,
                    payeeId: payeeId,
                    paymentProcessor: paymentProcessor,
                    paymentProcessorTransactionId: paymentProcessorTransactionId,
                    amountUsd: amount,
                    utcPaymentDate: dueDate)
                {
                    PaymentCollectorId = paymentCollectorId,
                    Note = note
                });

            return result;
        }

        public CommitInvoiceResponse CommitNexportInvoiceTransaction([NotNull] string url, [NotNull] string accessToken, Guid invoiceId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiCommitInvoiceTransaction(accessToken, new CommitInvoiceRequest(invoiceId));

            return result;
        }

        public InvoiceRedemptionResponse RedeemNexportInvoice([NotNull] string url, [NotNull] string accessToken, Guid invoiceItemId,
            Guid redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiRedeemInvoiceItem(accessToken,
                    new RedeemInvoiceItemRequest(invoiceItemId, redeemingUserId: redeemingUserId, redemptionActionType: redemptionAction));

            return result;
        }

        public InvoiceRedemptionResponse GetNexportInvoiceRedemption([NotNull] string url, [NotNull] string accessToken, Guid invoiceItemId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.PointOfSaleApiGetInvoiceRedemption(invoiceItemId, accessToken);

            return result;
        }

        public SsoResponse NexportSingleSignOn([NotNull] string url, [NotNull] string accessToken, Guid orgId, Guid userId, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.SsoApiSignIn(accessToken,
                    new SsoRequest(Enums.DisplayEnum.Normal, userId, orgId,
                    redirectEntityRequest: new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)));

            return result;
        }

        public SsoResponse NexportClassroomSingleSignOn([NotNull] string url, [NotNull] string accessToken, Guid enrollmentId, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.SsoApiClassroom(accessToken,
                    new ClassroomSsoRequest(Enums.DisplayEnum.Normal, enrollmentId,
                    new RedirectEntityRequest(redirectUrl, redirectUrl, redirectUrl)));

            return result;
        }

        public SectionEnrollmentsResponse GetNexportSectionEnrollment([NotNull] string url, [NotNull] string accessToken, Guid orgId, Guid userId, Guid syllabusId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var apiResult = nexportApi.LearningApiGetSectionEnrollments(accessToken, orgId, null, userId, syllabusId);

            var result = apiResult.FirstOrDefault();

            return result;
        }

        public TrainingPlanEnrollmentsResponse GetNexportTrainingPlanEnrollment([NotNull] string url,
            [NotNull] string accessToken, Guid orgId, Guid userId, Guid syllabusId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.LearningApiGetTrainingPlanEnrollments(accessToken, orgId, userId: userId, syllabusId: syllabusId)
                .FirstOrDefault();

            return result;
        }

        public CreateMembershipResponse CreateNexportMemberships([NotNull] string url, [NotNull] string accessToken, Guid userId, IList<Guid> groupIds)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.AdminApiCreateMembership(accessToken,
                new CreateMembershipRequest(groupIds.ToList(), userId));

            return result;
        }

        public RemoveMembershipResponse RemoveNexportMemberships([NotNull] string url, [NotNull] string accessToken, IList<Guid> membershipIds)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.AdminApiRemoveMembership(accessToken,
                new RemoveMembershipRequest(memberships: membershipIds.ToList()));

            return result;
        }

        public SetCustomProfileFieldValuesResponse SetNexportCustomerProfileFieldValues([NotNull] string url, [NotNull] string accessToken,
            Guid subscriberId, Dictionary<string, string> profileFields)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("Api url cannot be empty");

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new NullReferenceException("Access token cannot be empty");

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApi(_apiConfiguration)
            {
                Client = EngineContext.Current.Resolve<ISynchronousClient>(),
                AsynchronousClient = EngineContext.Current.Resolve<IAsynchronousClient>()
            };

            var result = nexportApi.AdminApiSetCustomProfileFieldValues(accessToken,
                new SetCustomProfileFieldValuesRequest(subscriberId, profileFields));

            return result;
        }
    }
}
