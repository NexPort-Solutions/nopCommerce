using System;
using System.Collections.Generic;
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

        public NexportAuthenticationResponseDetails AuthenticateNexportApi(string url, string username, string password, DateTime? tokenExp)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiAuthenticateWithHttpInfo(new AuthenticationTokenRequest(username, password,
                "password", utcExpirationDate: tokenExp));

            var result = new NexportAuthenticationResponseDetails()
            {
                Response = response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails AuthenticateNexportUser(string url, string accessToken, string loginName, string password)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiAuthenticateUserWithHttpInfo(loginName, password, accessToken);

            var result = new NexportGetUserResponseDetails()
            {
                Response =  response.Data,
                StatusCode =  response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByLogin(string url, string accessToken, string loginName)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, loginName);

            var result = new NexportGetUserResponseDetails()
            {
                Response =  response.Data,
                StatusCode =  response.StatusCode
            };

            return result;
        }

        public NexportGetUserResponseDetails GetNexportUserByUserId(string url, string accessToken, Guid userId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(_apiConfiguration);
            var response = nexportApi.AdminApiGetUserWithHttpInfo(accessToken, userId: userId.ToString());

            var result = new NexportGetUserResponseDetails()
            {
                Response =  response.Data,
                StatusCode =  response.StatusCode
            };

            return result;
        }

        public NexportUserListResponse GetNexportUsers(string url, string accessToken, string searchTerm, int? page = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

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

        public NexportCreateUserResponseDetails CreateNexportUser(string url, string accessToken,
            string login, string password,
            string firstName, string lastName, string email, Guid ownerOrgId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new AdminApiApi(url);
            var response = nexportApi.AdminApiCreateUserWithHttpInfo(
                new CreateUserRequest(ownerOrgId.ToString(), login, password, firstName, "", lastName, email), accessToken);

            var result = new NexportCreateUserResponseDetails()
            {
                Response = response.Data, StatusCode = response.StatusCode
            };

            return result;
        }

        public NexportDirectoryResponse SearchNexportDirectory(string url, string accessToken,
            Guid baseOrgId, string searchTerm, int? page = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

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

        public NexportOrganizationResponse GetNexportOrganizations(string url, string accessToken,
            Guid rootOrgId, int? page = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

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

        public NexportCatalogResponse GetNexportCatalogs(string url, string accessToken, Guid orgId,
            int? page = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var catalogRequest = new CatalogRequest(orgId.ToString(), CatalogRequest.PublishingModelEnum.ForSaleInMarketPlace, CatalogRequest.CatalogAccessOptionEnum.Owned);
            var response = nexportApi.LearningApiGetCatalogsWithHttpInfo(catalogRequest, accessToken, page: page);

            var result = new NexportCatalogResponse()
            {
                CatalogList = response.Data.Catalogs,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public CatalogResponseItem GetNexportCatalogDetails(string url, string accessToken, Guid catalogId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalog(catalogId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportCatalogDescription(string url, string accessToken,
            Guid catalogId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalogDescription(catalogId.ToString(), accessToken);

            return result;
        }

        public GetCatalogCreditHoursResponse GetNexportCatalogCreditHours(string url, string accessToken,
            Guid catalogId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetCatalogCreditHours(catalogId.ToString(), accessToken);

            return result;
        }

        public NexportSyllabusResponse GetNexportSyllabuses(string url, string accessToken, Guid catalogId,
            int? page = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var response = nexportApi.LearningApiGetCatalogSyllabiWithHttpInfo(catalogId.ToString(), accessToken, page: page);

            var result = new NexportSyllabusResponse()
            {
                SyllabusList = response.Data,
                TotalRecord = int.Parse(response.Headers["X-Total-Count"]),
                RecordPerPage = int.Parse(response.Headers["X-Per-Page"]),
                CurrentPage = int.Parse(response.Headers["X-Page"])
            };

            return result;
        }

        public SectionResponse GetNexportSectionDetails(string url, string accessToken, Guid sectionId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSection(sectionId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportSectionDescription(string url, string accessToken, Guid sectionId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSectionDescription(sectionId.ToString(), accessToken);

            return result;
        }

        public GetObjectivesResponse GetNexportSectionObjectives(string url, string accessToken, Guid sectionId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetSectionObjectives(sectionId.ToString(), accessToken);

            return result;
        }

        public TrainingPlanResponse GetNexportTrainingPlanDetails(string url, string accessToken,
            Guid trainingPlanId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetTrainingPlan(trainingPlanId.ToString(), accessToken);

            return result;
        }

        public GetDescriptionResponse GetNexportTrainingPlanDescription(string url, string accessToken,
            Guid trainingPlanId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new LearningApiApi(_apiConfiguration);
            var result = nexportApi.LearningApiGetTrainingPlanDescription(trainingPlanId.ToString(), accessToken);

            return result;
        }

        public NexportGetInvoiceResponseDetails GetNexportInvoice(string url, string accessToken, Guid invoiceId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var response = nexportApi.PointOfSaleApiGetInvoiceWithHttpInfo(invoiceId.ToString(), accessToken);

            var result = new NexportGetInvoiceResponseDetails
            {
                Response =  response.Data,
                StatusCode = response.StatusCode
            };

            return result;
        }

        public BeginInvoiceTransactionResponse BeginNexportInvoiceTransaction(string url, string accessToken,
            Guid orgId, Guid purchasingAgentId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result = nexportApi.PointOfSaleApiBeginInvoiceTransaction(
                new CreateInvoiceMessageRequest(purchasingAgentId.ToString(), orgId.ToString()), accessToken);

            return result;
        }

        public AddInvoiceItemResponse AddNexportInvoiceItem(string url, string accessToken,
            Guid invoiceId, Guid productId, Enums.ProductTypeEnum productType,
            Guid subscriptionOrgId, List<Guid> groupMembershipIds,
            decimal cost, string note = null, DateTime? accessExpirationDate = null,
            string accessExpirationTimeLimit = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

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

        public AddInvoiceScheduledPaymentResponse AddNexportInvoiceScheduledPayment(string url, string accessToken,
            string invoiceId, decimal amount, string note = null, DateTime? dueDate = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiAddInvoiceScheduledPayment(new InvoiceScheduledPaymentRequest()
                {
                    InvoiceId = invoiceId,
                    AmountUsd = amount,
                    Note = note,
                    UtcDueDate = dueDate
                }, accessToken);

            return result;
        }

        public AddInvoicePaymentResponse AddNexportInvoicePayment(string url, string accessToken,
            Guid invoiceId, decimal amount, Guid merchantAccountId,
            Guid payeeId, InvoicePaymentRequest.PaymentProcessorEnum paymentProcessor,
            string paymentProcessorTransactionId,
            DateTime dueDate,
            Guid? paymentCollectorId = null,
            string note = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

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

        public CommitInvoiceResponse CommitNexportInvoiceTransaction(string url, string accessToken,
            Guid invoiceId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiCommitInvoiceTransaction(new CommitInvoiceRequest(invoiceId.ToString()), accessToken);

            return result;
        }

        public InvoiceRedemptionResponse RedeemNexportInvoice(string url, string accessToken, string invoiceItemId,
            Guid redeemingUserId, RedeemInvoiceItemRequest.RedemptionActionTypeEnum redemptionAction)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiRedeemInvoiceItem(
                    new RedeemInvoiceItemRequest(invoiceItemId, redeemingUserId: redeemingUserId.ToString(), redemptionActionType: redemptionAction), accessToken);

            return result;
        }

        public InvoiceRedemptionResponse GetNexportInvoiceRedemption(string url, string accessToken, string invoiceItemId)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new PointOfSaleApiApi(_apiConfiguration);
            var result =
                nexportApi.PointOfSaleApiGetInvoiceRedemption(invoiceItemId, accessToken);

            return result;
        }

        public SsoResponse NexportSingleSignOn(string url, string accessToken, string orgId, string userId, string redirectUrl)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApiApi(_apiConfiguration);
            var result =
                nexportApi.SsoApiSignIn(new SsoRequest()
                {
                    Display = SsoRequest.DisplayEnum.Normal,
                    OrgId = orgId,
                    RedirectEntityRequest = new RedirectEntityRequest()
                    {
                        LogoutUrl = redirectUrl,
                        ExpiredTokenUrl = redirectUrl,
                        InvalidTokenUrl = redirectUrl
                    },
                    UserId = userId
                }, accessToken);

            return result;
        }

        public SsoResponse NexportClassroomSingleSignOn(string url, string accessToken, string enrollmentId, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new NullReferenceException("Api url cannot be empty");
            }

            _apiConfiguration.BasePath = url;

            var nexportApi = new SsoApiApi(_apiConfiguration);
            var result =
                nexportApi.SsoApiClassroom(new ClassroomSsoRequest()
                {
                    Display = ClassroomSsoRequest.DisplayEnum.Normal,
                    EnrollmentId = enrollmentId,
                    RedirectEntityRequest = new RedirectEntityRequest()
                    {
                        LogoutUrl = redirectUrl,
                        ExpiredTokenUrl = redirectUrl,
                        InvalidTokenUrl = redirectUrl
                    }
                }, accessToken);

            return result;
        }
    }
}
