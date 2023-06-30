using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Extensions;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public class NexportPluginModelFactory : INexportPluginModelFactory
    {
        #region Fields

        private readonly NexportSettings _nexportSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly NexportService _nexportService;
        private readonly IAddressService _addressService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Constructor

        public NexportPluginModelFactory(
            NexportSettings nexportSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IProductService productService,
            IStoreService storeService,
            IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
            IWorkContext workContext,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            ILogger logger,
            NexportService nexportService,
            IPaymentPluginManager paymentPluginManager,
            IAddressService addressService,
            IPriceFormatter priceFormatter)
        {
            _nexportSettings = nexportSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _productService = productService;
            _storeService = storeService;
            _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
            _workContext = workContext;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _logger = logger;
            _nexportService = nexportService;
            _paymentPluginManager = paymentPluginManager;
            _addressService = addressService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        public virtual async Task<NexportProductMappingModel> PrepareNexportProductMappingModelAsync(NexportProductMapping productMapping, bool isEditable)
        {
            var model = productMapping.ToModel<NexportProductMappingModel>();
            model.Editable = isEditable;
            if (model.StoreId != null)
            {
                model.StoreName = await _nexportService.GetStoreNameAsync(model.StoreId.Value);
            }

            if (model.NexportSyllabusId != null)
            {
                if (model.Type == NexportProductTypeEnum.Section)
                {
                    var sectionDetails = await _nexportService.GetSectionDetailsAsync(model.NexportSyllabusId.Value);
                    model.SectionNumber = sectionDetails?.SectionNumber;
                    model.UniqueName = sectionDetails?.UniqueName;
                }
                else if (model.Type == NexportProductTypeEnum.TrainingPlan)
                {
                    var trainingPlanDetails = await _nexportService.GetTrainingPlanDetailsAsync(model.NexportSyllabusId.Value);
                    model.UniqueName = trainingPlanDetails?.UniqueName;
                }
            }

            if (model.NexportCatalogId != default)
            {
                model.NexportCatalogName =
                    (await _nexportService.GetCatalogDetailsAsync(productMapping.NexportCatalogId)).Name;
            }

            model.SupplementalInfoQuestionIds =
                (await _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id))
                    .Select(x => x.QuestionId).ToList();

            var availableSupplementalInfoQuestions = await _nexportService.GetSupplementalInfoQuestionList();
            foreach (var questionItem in availableSupplementalInfoQuestions)
            {
                model.AvailableSupplementalInfoQuestions.Add(questionItem);
            }

            foreach (var questionItem in model.AvailableSupplementalInfoQuestions)
            {
                questionItem.Selected = int.TryParse(questionItem.Value, out var questionId) &&
                                        model.SupplementalInfoQuestionIds.Contains(questionId);
            }

            return model;
        }

        /// <summary>
        /// Prepare product search model to add to the order
        /// </summary>
        /// <param name="searchModel">Product search model to add to the order</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model to add to the order
        /// </returns>
        public virtual async Task<NexportProductMappingListSearchModel> PrepareNexportProductMappingListSearchModelAsync(NexportProductMappingListSearchModel searchModel, ProductModel productModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productModel == null)
                throw new ArgumentNullException(nameof(productModel));

            searchModel.NopProductId = productModel.Id;

            //prepare available product types
            searchModel.AvailableNexportProductTypes.Add(new SelectListItem { Value = null, Text = "All" });

            //for each nexportproducttype enum create a selectlistitem and add it for the product type filter
            foreach (var e in Enum.GetValues(typeof(NexportProductTypeEnum)))
            {
                searchModel.AvailableNexportProductTypes.Add(new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.GetDisplayName()
                });
            }

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<NexportProductMappingListModel> PrepareNexportProductMappingListModelAsync(
            NexportProductMappingListSearchModel searchModel, int nopProductId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var mappings = await _nexportService.GetAllNexportProductMappingsAsync(searchProductName: searchModel.SearchNexportProductName,
                searchProductType: searchModel.NexportProductType,
                searchStoreName: searchModel.SearchStoreName,
                productId: nopProductId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            // Prepare grid model
            var model = await new NexportProductMappingListModel().PrepareToGridAsync(searchModel, mappings, () =>
            {
                return mappings.SelectAwait(async mapping =>
                {
                    // Fill in model values from the entity
                    var mappingModel = mapping.ToModel<NexportProductMappingModel>();
                    if (mappingModel.StoreId.HasValue)
                    {
                        mappingModel.StoreName = await _nexportService.GetStoreNameAsync(mappingModel.StoreId.Value);
                    }

                    if (mappingModel.NexportSyllabusId != null)
                    {
                        if (mappingModel.Type == NexportProductTypeEnum.Section)
                        {
                            var sectionDetails =
                                await _nexportService.GetSectionDetailsAsync(mappingModel.NexportSyllabusId.Value);
                            mappingModel.SectionNumber = sectionDetails?.SectionNumber;
                            mappingModel.UniqueName = sectionDetails?.UniqueName;
                        }
                        else if (mappingModel.Type == NexportProductTypeEnum.TrainingPlan)
                        {
                            var trainingPlanDetails =
                                await _nexportService.GetTrainingPlanDetailsAsync(mappingModel.NexportSyllabusId.Value);
                            mappingModel.UniqueName = trainingPlanDetails?.UniqueName;
                        }
                    }

                    if (mappingModel.NexportCatalogId != default)
                    {
                        mappingModel.NexportCatalogName =
                            (await _nexportService.GetCatalogDetailsAsync(mapping.NexportCatalogId)).Name;
                    }
                    var groupMemberships =
                        await _nexportService.GetProductGroupMembershipMappings(mappingModel.Id);
                    foreach (var groupMembership in groupMemberships)
                    {
                        var groupMembershipModel = groupMembership.ToModel<NexportProductGroupMembershipMappingModel>();
                        mappingModel.GroupMembershipMappingModels.Add(groupMembershipModel);
                    }

                    return mappingModel;
                });
            });

            return model;
        }

        public virtual async Task<NexportProductGroupMembershipMappingListModel>
            PrepareNexportProductMappingGroupMembershipListModelAsync(
                NexportProductGroupMembershipMappingListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var groupMembershipMappings =
                await _nexportService.GetProductGroupMembershipMappingsPagination(searchModel.NexportProductMappingId,
                    searchModel.Page - 1, searchModel.PageSize);

            var model = new NexportProductGroupMembershipMappingListModel().PrepareToGrid(searchModel, groupMembershipMappings, () =>
            {
                return groupMembershipMappings.Select(mapping =>
                {
                    var mappingModel = mapping.ToModel<NexportProductGroupMembershipMappingModel>();

                    return mappingModel;
                });
            });

            return model;
        }

        public async Task<DuplicateNexportProductMappingModel> PrepareDuplicateNexportProductMappingModel(Product product)
        {
            var model = new DuplicateNexportProductMappingModel();

            var defaultMapping = await _nexportService.GetProductMappingByNopProductId(product.Id);
            if (defaultMapping == null)
                return model;

            model.AvailableStores.Add(new SelectListItem
            {
                Text = "Default",
                Value = ""
            });

            var availableStores = await _storeService.GetAllStoresAsync();
            foreach (var store in availableStores)
            {
                var mapping = await _nexportService.GetProductMappingByNopProductId(product.Id, store.Id);
                if (mapping != null)
                {
                    model.AvailableStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString()
                    });
                }
                else
                {
                    model.DestinationStores.Add(new SelectListItem
                    {
                        Text = store.Name,
                        Value = store.Id.ToString()
                    });
                }
            }

            return model;
        }

        public virtual async Task<NexportCustomerAdditionalInfoModel> PrepareNexportAdditionalInfoModelAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var model = new NexportCustomerAdditionalInfoModel { CustomerId = customer.Id };
            var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
            if (nexportUserMapping != null)
            {
                model.NexportUserId = nexportUserMapping.NexportUserId;
            }

            model.NexportSupplementalInfoAnswerListSearchModel = new NexportSupplementalInfoAnswerListSearchModel
            {
                CustomerId = customer.Id,
            };

            await _baseAdminModelFactory.PrepareStoresAsync(model.NexportSupplementalInfoAnswerListSearchModel.AvailableStores);

            model.NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel = new NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel
            {
                CustomerId = customer.Id
            };

            var availableStores = await _storeService.GetAllStoresAsync();

            model.NexportCustomerRegistrationFieldWithAnswersListSearchModel = new NexportCustomerRegistrationFieldWithAnswersListSearchModel
            {
                CustomerId = customer.Id,
                AvailableStores = availableStores.Select(store => new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString()
                }).ToList()
            };

            model.NexportCustomerRegistrationFieldAnswerListSearchModel = new NexportCustomerRegistrationFieldAnswerListSearchModel
            {
                CustomerId = customer.Id
            };

            return model;
        }

        public async Task<AddNexportCustomerAdditionalInfoModel> PrepareAddNexportAdditionalInfoModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var model = new AddNexportCustomerAdditionalInfoModel { CustomerId = customer.Id };

            var availableStores = await _storeService.GetAllStoresAsync();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString()
            }).ToList();

            model.AvailableStores.Insert(0, new SelectListItem("Select store", ""));

            return model;
        }

        public virtual async Task<NexportCatalogListModel> PrepareNexportCatalogListModelAsync(NexportCatalogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var catalogs =
                await _nexportService.FindAllCatalogsAsync(searchModel.OrgId, searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = new NexportCatalogListModel().PrepareToGrid(searchModel, catalogs, () =>
            {
                return catalogs.Select(catalog =>
                {
                    // Fill in model values from the entity
                    var catalogItemModel = new NexportCatalogResponseItemModel()
                    {
                        OrgId = catalog.OrgId,
                        CatalogId = catalog.CatalogId,
                        IsEnabled = catalog.IsEnabled,
                        Name = catalog.Name,
                        OwnerName = catalog.OwnerName,
                        PricingModel = catalog.PricingModel,
                        PublishingModel = catalog.PublishingModel,
                        UtcDateCreated = catalog.DateCreated,
                        UtcDateLastModified = catalog.LastModified,
                        AccessTimeLimit = catalog.AccessTimeLimit
                    };

                    return catalogItemModel;
                });
            });

            return model;
        }

        public virtual async Task<NexportSyllabusListModel> PrepareNexportSyllabusListModelAsync(NexportSyllabusListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var syllabus = await _nexportService.FindAllSyllabusesAsync(searchModel.CatalogId, searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = await new NexportSyllabusListModel().PrepareToGridAsync(searchModel, syllabus, () =>
            {
                return syllabus.SelectAwait(async syllabi =>
                {
                    // Fill in model values from the entity
                    var syllabiId = syllabi.SyllabusId;
                    var syllabiItemModel = new NexportSyllabiResponseItemModel()
                    {
                        CatalogId = searchModel.CatalogId,
                        SyllabusId = syllabiId,
                        Name = syllabi.SyllabusName,
                        Type = syllabi.SyllabusType,
                        ProductId = syllabi.ProductId.Value,
                        TotalMappings = await _nexportService.FindMappingCountPerSyllabi(syllabiId)
                    };

                    if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.Section)
                    {
                        var sectionDetails = await _nexportService.GetSectionDetailsAsync(syllabi.SyllabusId);
                        if (sectionDetails != null)
                        {
                            syllabiItemModel.UniqueName = sectionDetails.UniqueName;
                            syllabiItemModel.SectionNumber = sectionDetails.SectionNumber;
                        }
                    }
                    else if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.TrainingPlan)
                    {
                        var trainingPlanDetails = await _nexportService.GetTrainingPlanDetailsAsync(syllabi.SyllabusId);
                        if (trainingPlanDetails != null)
                        {
                            syllabiItemModel.UniqueName = trainingPlanDetails.UniqueName;
                        }
                    }

                    return syllabiItemModel;
                });
            });

            return model;
        }

        public virtual Task<NexportLoginModel> PrepareNexportLoginModelAsync(bool? checkoutAsGuest)
        {
            return Task.FromResult(new NexportLoginModel
            {
                UsernamesEnabled = false,
                RegistrationType = _customerSettings.UserRegistrationType,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage
            });
        }

        public virtual async Task<NexportTrainingListModel> PrepareNexportTrainingListModelAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
            var redemptionOrganizations =
                await _nexportService.FindNexportRedemptionOrganizationsByCustomerId(customer.Id);

            var model = new NexportTrainingListModel();

            if (userMapping != null)
            {
                model.UserId = userMapping.NexportUserId;
                model.RedemptionOrganizations = redemptionOrganizations;

                var customerOrderInvoices = (await _nexportService.GetNexportOrderInvoiceItems(userMapping.NexportUserId))
                    .Where(x => x.InvoiceItemId != Guid.Empty)
                    .GroupBy(x => x.RedemptionEnrollmentId)
                    .Select(x => x.OrderByDescending(invoice => invoice.UtcDateRedemption).First())
                    .OrderByDescending(x => x.UtcDateRedemption)
                    .ToList();
                var trainingList = new List<NexportTrainingItemModel>();
                foreach (var orderInvoice in customerOrderInvoices)
                {
                    try
                    {
                        var nexportInvoiceDetails =
                            await _nexportService.GetNexportInvoiceRedemptionAsync(orderInvoice.InvoiceItemId);
                        if (nexportInvoiceDetails?.UtcRedemptionDate != null)
                        {
                            DateTime? enrollmentStartDate = null;
                            DateTime? enrollmentExpirationDate = null;
                            var enrollmentStatus = Enums.PhaseEnum.NotStarted;
                            if (nexportInvoiceDetails.RedemptionUserId != null)
                            {
                                var enrollmentExisted = false;

                                try
                                {
                                    if (nexportInvoiceDetails.RedemptionType == null ||
                                    nexportInvoiceDetails.RedemptionType ==
                                    InvoiceRedemptionResponse.RedemptionTypeEnum.Section)
                                    {
                                        var enrollmentDetails = await _nexportService.GetSectionEnrollmentDetailsAsync(
                                            nexportInvoiceDetails.OrganizationId,
                                            nexportInvoiceDetails.RedemptionUserId.Value, nexportInvoiceDetails.SyllabusId);
                                        if (enrollmentDetails != null)
                                        {
                                            enrollmentExisted = true;
                                            enrollmentStartDate = enrollmentDetails.EnrollmentDate;
                                            enrollmentExpirationDate = enrollmentDetails.ExpirationDate;
                                            enrollmentStatus = enrollmentDetails.Phase;
                                        }
                                    }
                                    else if (nexportInvoiceDetails.RedemptionType ==
                                             InvoiceRedemptionResponse.RedemptionTypeEnum.TrainingPlan)
                                    {
                                        var enrollmentDetails = await _nexportService.GetTrainingPlanEnrollmentDetailsAsync(
                                            nexportInvoiceDetails.OrganizationId,
                                            nexportInvoiceDetails.RedemptionUserId.Value, nexportInvoiceDetails.SyllabusId);
                                        if (enrollmentDetails != null)
                                        {
                                            enrollmentExisted = true;
                                            enrollmentStartDate = enrollmentDetails.EnrollmentDate;
                                            enrollmentExpirationDate = enrollmentDetails.ExpirationDate;
                                            enrollmentStatus = enrollmentDetails.Phase;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await _logger.WarningAsync($"Unable to get syllabus details for syllabus {nexportInvoiceDetails.SyllabusId}", ex);
                                }

                                if (enrollmentExisted)
                                {
                                    var trainingItem = new NexportTrainingItemModel
                                    {
                                        Name = nexportInvoiceDetails.SyllabusTitle,
                                        Type = nexportInvoiceDetails.RedemptionType ?? InvoiceRedemptionResponse.RedemptionTypeEnum.Section,
                                        UtcStartDate = enrollmentStartDate,
                                        UtcExpirationDate = enrollmentExpirationDate,
                                        UtcRedemptionDate = nexportInvoiceDetails.UtcRedemptionDate,
                                        EnrollmentId = nexportInvoiceDetails.RedemptionEnrollmentId,
                                        SyllabusId = nexportInvoiceDetails.SyllabusId,
                                        OrganizationId = nexportInvoiceDetails.OrganizationId,
                                        Status = enrollmentStatus
                                    };

                                    if (!trainingList.Any(x => x.SyllabusId == nexportInvoiceDetails.SyllabusId))
                                        trainingList.Add(trainingItem);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.WarningAsync($"Unable to get Nexport invoice item {orderInvoice.InvoiceItemId}", ex, customer);
                    }
                }

                model.Trainings = trainingList
                    .GroupBy(x => x.OrganizationId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.ToList());
            }

            return model;
        }

        public async Task<NexportCustomerSupplementalInfoAnswersModel>
            PrepareNexportCustomerSupplementalInfoAnswersModelAsync(Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new NexportCustomerSupplementalInfoAnswersModel();

            var answers = await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
            var questionIds = answers.Select(a => a.QuestionId).Distinct().ToList();

            foreach (var questionId in questionIds)
            {
                var answerWithOptionsList = answers.Where(a => a.QuestionId == questionId);
                var answerWithOptionsDictionary = answerWithOptionsList.ToDictionary(
                    answerAndOption => answerAndOption.Id,
                    answerAndOption => answerAndOption.OptionId);
                model.QuestionWithAnswersList.Add(questionId, answerWithOptionsDictionary);
            }

            return model;
        }

        public async Task<NexportCustomerSupplementalInfoAnswerEditModel>
            PrepareNexportCustomerSupplementalInfoAnswersEditModelAsync(Customer customer, Store store,
                NexportSupplementalInfoQuestion question)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var currentAnswers = (await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id, question.Id))
                .Select(currentAnswer => new EditSupplementInfoAnswerRequest
                {
                    AnswerId = currentAnswer.Id,
                    OptionId = currentAnswer.OptionId
                }).ToList();

            var model = new NexportCustomerSupplementalInfoAnswerEditModel
            {
                Question = question,
                Options = await _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id, true),
                Answers = currentAnswers
            };

            return model;
        }

        public async Task<NexportCustomerSupplementalInfoAnsweredQuestionListModel>
            PrepareNexportSupplementalInfoQuestionListModelAsync(
                NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerSupplementalInfoAnsweredQuestions = await _nexportService.GetNexportSupplementalInfoAnsweredQuestionsPagination(searchModel.CustomerId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new NexportCustomerSupplementalInfoAnsweredQuestionListModel().PrepareToGrid(searchModel,
                customerSupplementalInfoAnsweredQuestions, () =>
            {
                return customerSupplementalInfoAnsweredQuestions.Select(question =>
                {
                    var questionModel = question.ToModel<NexportCustomerSupplementalInfoAnsweredQuestionModel>();

                    return questionModel;
                });
            });

            return model;
        }

        public async Task<NexportSupplementalInfoAnswerListModel> PrepareNexportSupplementalInfoAnswerListModelAsync(
            NexportSupplementalInfoAnswerListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerSupplementalInfoAnswers = await _nexportService.GetNexportSupplementalInfoAnswersPagination(
                searchModel.CustomerId,
                searchModel.QuestionId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = await new NexportSupplementalInfoAnswerListModel().PrepareToGridAsync(searchModel, customerSupplementalInfoAnswers, () =>
            {
                return customerSupplementalInfoAnswers.SelectAwait(async answer =>
                {
                    var answerModel = answer.ToModel<NexportSupplementalInfoAnswerModel>();

                    answerModel.StoreName = (await _storeService.GetStoreByIdAsync(answer.StoreId)).Name;
                    answerModel.OptionText =
                        (await _nexportService.GetNexportSupplementalInfoOptionById(answer.OptionId)).OptionText;
                    answerModel.NexportMemberships = (await _nexportService
                        .GetNexportSupplementalInfoAnswerMembershipsByAnswerId(answer.Id))
                        .Select(am => am.NexportMembershipId).ToList();

                    return answerModel;
                }).OrderBy(a => a.StoreName);
            });

            return model;
        }

        public virtual Task<NexportSupplementalInfoQuestionSearchModel>
            PrepareNexportSupplementalInfoQuestionSearchModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<NexportSupplementalInfoQuestionListModel>
            PrepareNexportSupplementalInfoQuestionListModelAsync(NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get all available supplemental info questions
            var supplementalInfoQuestions =
                await _nexportService.GetAllNexportSupplementalInfoQuestionsPagination(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            // Prepare the list model
            var model = new NexportSupplementalInfoQuestionListModel().PrepareToGrid(searchModel,
                supplementalInfoQuestions, () =>
                {
                    return supplementalInfoQuestions.Select(question =>
                    {
                        //fill in model values from the entity
                        var requestModel = question.ToModel<NexportSupplementalInfoQuestionModel>();
                        return requestModel;
                    });
                });

            return model;
        }

        public virtual async Task<NexportSupplementalInfoQuestionModel>
            PrepareNexportSupplementalInfoQuestionModelAsync(NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question)
        {
            if (question != null)
            {
                model ??= question.ToModel<NexportSupplementalInfoQuestionModel>();

                await PrepareNexportSupplementalInfoOptionSearchModelAsync(model.NexportSupplementalInfoOptionSearchModel, question);
            }

            var availableQuestionTypes = new List<SelectListItem>
            {
                new()
                {
                    Text = NexportSupplementalInfoQuestionType.SingleOption.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.SingleOption).ToString(),
                    Selected = true
                },
                new()
                {
                    Text = NexportSupplementalInfoQuestionType.MultipleOptions.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.MultipleOptions).ToString()
                }
            };

            foreach (var type in availableQuestionTypes)
                model.AvailableQuestionTypes.Add(type);

            return model;
        }

        public virtual Task<NexportSupplementalInfoOptionSearchModel>
            PrepareNexportSupplementalInfoOptionSearchModelAsync(NexportSupplementalInfoOptionSearchModel searchModel,
                NexportSupplementalInfoQuestion question)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            searchModel.QuestionId = question.Id;

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<NexportSupplementalInfoOptionListModel>
            PrepareNexportSupplementalInfoOptionListModelAsync(NexportSupplementalInfoOptionSearchModel searchModel,
                NexportSupplementalInfoQuestion question)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var options = (await _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id))
                .ToPagedList(searchModel);

            //prepare list model
            var model = new NexportSupplementalInfoOptionListModel().PrepareToGrid(searchModel, options, () =>
            {
                return options.Select(option =>
                {
                    //fill in model values from the entity
                    var optionModel = option.ToModel<NexportSupplementalInfoOptionModel>();

                    return optionModel;
                });
            });

            return model;
        }

        public virtual Task<NexportSupplementalInfoOptionModel> PrepareNexportSupplementalInfoOptionModelAsync(
            NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question,
            NexportSupplementalInfoOption option)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            if (option != null)
            {
                //fill in model values from the entity
                model ??= option.ToModel<NexportSupplementalInfoOptionModel>();
            }

            model.QuestionId = question.Id;

            return Task.FromResult(model);
        }

        public async Task<NexportSupplementalInfoOptionGroupAssociationListModel>
            PrepareNexportSupplementalInfoOptionGroupAssociationListModelAsync(
                NexportSupplementalInfoOptionGroupAssociationListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var groupAssociations =
                await _nexportService.GetNexportSupplementalInfoOptionGroupAssociationsPagination(searchModel.OptionId,
                    searchModel.Page - 1, searchModel.PageSize);

            var model = new NexportSupplementalInfoOptionGroupAssociationListModel().PrepareToGrid(searchModel, groupAssociations, () =>
            {
                return groupAssociations.Select(mapping =>
                {
                    var mappingModel = mapping.ToModel<NexportSupplementalInfoOptionGroupAssociationModel>();

                    return mappingModel;
                });
            });

            return model;
        }

        public async Task<NexportSupplementalInfoAnswerQuestionModel> PrepareNexportSupplementalInfoAnswerQuestionModelAsync(
            IList<int> questionIds, Customer customer, Store store)
        {
            if (questionIds == null)
                throw new ArgumentNullException(nameof(questionIds));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new NexportSupplementalInfoAnswerQuestionModel();

            var questionWithoutAnswerIds = new List<int>();

            var answeredQuestions = new Dictionary<int, string>();

            var answers = await _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
            var answered = answers.Where(x => questionIds.Contains(x.QuestionId)).ToList();

            var questionWithAnswerIds = answered.Count > 0
                ? answered.Select(x => x.QuestionId).ToList()
                : new List<int>();

            questionWithoutAnswerIds.AddRange(questionIds.Except(questionWithAnswerIds));

            foreach (var answer in answered.Where(answer => !answeredQuestions.ContainsKey(answer.Id)))
            {
                answeredQuestions.Add(answer.Id, $"{answer.QuestionId},{answer.OptionId}");
            }

            model.QuestionIds = questionIds.Distinct().ToList();
            model.QuestionWithoutAnswerIds = questionWithoutAnswerIds;

            return model;
        }

        public async Task<NexportCustomerAdditionalSettingsModel> PrepareNexportCustomerAdditionalSettingsModelAsync()
        {
            var model = new NexportCustomerAdditionalSettingsModel();

            model.NexportRegistrationFieldCategorySearchModel.SetGridPageSize();
            model.NexportRegistrationFieldSearchModel =
                await PrepareNexportRegistrationFieldSearchModelAsync(model.NexportRegistrationFieldSearchModel);

            return model;
        }

        public async Task<NexportRegistrationFieldSearchModel> PrepareNexportRegistrationFieldSearchModelAsync(NexportRegistrationFieldSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare all the stores for the store search filter
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<NexportRegistrationFieldCategoryListModel> PrepareNexportRegistrationFieldCategoryListModelAsync(
            NexportRegistrationFieldCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFields =
                await _nexportService.GetNexportRegistrationFieldCategoriesPagination(searchModel.Page - 1, searchModel.PageSize);

            var model = new NexportRegistrationFieldCategoryListModel().PrepareToGrid(searchModel,
                registrationFields, () =>
                {
                    return registrationFields.Select(field =>
                    {
                        var fieldModel = field.ToModel<NexportRegistrationFieldCategoryModel>();
                        return fieldModel;
                    });
                });

            return model;
        }

        public Task<NexportRegistrationFieldCategoryModel> PrepareNexportRegistrationFieldCategoryModelAsync(
            NexportRegistrationFieldCategoryModel model, NexportRegistrationFieldCategory registrationFieldCategory)
        {
            if (registrationFieldCategory != null)
            {
                model ??= registrationFieldCategory.ToModel<NexportRegistrationFieldCategoryModel>();
            }

            return Task.FromResult(model);
        }

        public Task<NexportRegistrationFieldOptionSearchModel> PrepareNexportRegistrationFieldOptionSearchModelAsync(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (registrationField == null)
                throw new ArgumentNullException(nameof(registrationField));

            searchModel.RegistrationFieldId = registrationField.Id;

          //  searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<NexportRegistrationFieldListModel> PrepareNexportRegistrationFieldListModelAsync(
            NexportRegistrationFieldSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFields =
                await _nexportService.GetNexportRegistrationFieldsPagination(searchModel.SelectedStoreIds, searchModel.Page - 1, searchModel.PageSize);

            var model = await new NexportRegistrationFieldListModel().PrepareToGridAsync(searchModel,
                registrationFields, () =>
                {
                    return registrationFields.SelectAwait(async field =>
                    {
                        var fieldModel = field.ToModel<NexportRegistrationFieldModel>();

                        if (fieldModel.FieldCategoryId.HasValue)
                            fieldModel.FieldCategoryName =
                                (await _nexportService.GetNexportRegistrationFieldCategoryById(
                                    fieldModel.FieldCategoryId.Value)).Title;

                        var storeMappings = await _nexportService
                            .GetNexportRegistrationFieldStoreMappings(fieldModel.Id);

                        for (var i = 0; i < storeMappings.Count; i++)
                        {
                            var store = await _storeService.GetStoreByIdAsync(storeMappings[i].StoreId);
                            if (store != null)
                            {
                                if (i < storeMappings.Count - 1)
                                    fieldModel.StoreMappings += $"{store.Name}, ";
                                else
                                    fieldModel.StoreMappings += $"{store.Name}";
                                fieldModel.StoreMappingIds.Add(store.Id);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(fieldModel.CustomFieldRender))
                        {
                            var customRenderPlugin =
                                await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(fieldModel
                                    .CustomFieldRender);
                            fieldModel.CustomFieldRenderDescription = customRenderPlugin?.PluginDescriptor.FriendlyName;
                        }

                        return fieldModel;
                    });
                });

            return model;
        }

        public virtual async Task<NexportRegistrationFieldModel> PrepareNexportRegistrationFieldModelAsync(
            NexportRegistrationFieldModel model,
            NexportRegistrationField registrationField, bool excludeProperties = false)
        {
            Func<NexportRegistrationFieldLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (registrationField != null)
            {
                model ??= registrationField.ToModel<NexportRegistrationFieldModel>();

                model.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(registrationField,
                    nameof(model.AllowMultipleSelection), defaultValue: false);

                model.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(registrationField,
                    nameof(model.DisplayOptionByAscendingOrder), defaultValue: false);

                model.StoreMappingIds = (await _nexportService.GetNexportRegistrationFieldStoreMappings(registrationField.Id))
                    .Select(s => s.StoreId).ToList();

                await PrepareNexportRegistrationFieldOptionSearchModelAsync(model.RegistrationFieldOptionSearchModel, registrationField);

                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(registrationField,
                        entity => entity.Name, languageId,
                        false, false);
                };
            }

            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            model.AvailableFieldCategory = await _nexportService.GetRegistrationFieldCategoryList();

            var availableStores = await _storeService.GetAllStoresAsync();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
                Selected = model.StoreMappingIds.Contains(store.Id)
            }).ToList();

            model.AvailableCustomFieldRenders = await _nexportService.GetCustomRegistrationFieldRendersAsync();

            return model;
        }

        public virtual async Task<NexportRegistrationFieldOptionListModel>
            PrepareNexportRegistrationFieldOptionListModelAsync(NexportRegistrationFieldOptionSearchModel searchModel,
                NexportRegistrationField registrationField)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFieldOptions =
                await _nexportService.GetNexportRegistrationFieldOptionsPagination(registrationField.Id,
                    searchModel.Page - 1, searchModel.PageSize);

            var model = new NexportRegistrationFieldOptionListModel().PrepareToGrid(searchModel,
                registrationFieldOptions, () =>
                {
                    return registrationFieldOptions.Select(fieldOption =>
                    {
                        var fieldOptionModel = fieldOption.ToModel<NexportRegistrationFieldOptionModel>();
                        return fieldOptionModel;
                    });
                });

            return model;
        }

        public Task<NexportRegistrationFieldOptionModel> PrepareNexportRegistrationFieldOptionModelAsync(
            NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
            NexportRegistrationFieldOption registrationFieldOption)
        {
            if (registrationFieldOption != null)
            {
                model ??= registrationFieldOption.ToModel<NexportRegistrationFieldOptionModel>();
            }

            return Task.FromResult(model);
        }

        public async Task<NexportCustomerRegistrationFieldsModel> PrepareNexportCustomerRegistrationFieldsModelAsync(
            Store store)
        {
            var model = new NexportCustomerRegistrationFieldsModel();

            var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);

            var fieldsWithCategory = (await availableFields.Where(x => x.FieldCategoryId != null)
                .GroupByAwait(async x =>
                {
                    var fieldCategory = await _nexportService.GetNexportRegistrationFieldCategoryById(x.FieldCategoryId.Value);
                    return fieldCategory;
                })
                .ToDictionaryAwaitAsync(
                    async x => x.Key.ToModel<NexportRegistrationFieldCategoryModel>(),
                    x => x
                        .SelectAwait(async f =>
                            {
                                var fieldModel = f.ToModel<NexportRegistrationFieldModel>();
                                if (fieldModel.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
                                {
                                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                                        fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(f,
                                            nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(f,
                                        nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                                }

                                return fieldModel;
                            }).OrderBy(f => f.DisplayOrder).ToListAsync()))
                .OrderBy(x => x.Key.DisplayOrder)
                .ThenBy(x => x.Key.Title);

            model.RegistrationFieldsWithCategory = fieldsWithCategory.ToDictionary(
                x => x.Key,
                x => x.Value);

            model.RegistrationFieldsWithoutCategory = await availableFields
                .Where(x => x.FieldCategoryId == null)
                .OrderBy(x => x.DisplayOrder)
                .SelectAwait(async f =>
                {
                    var fieldModel = f.ToModel<NexportRegistrationFieldModel>();
                    if (fieldModel.Type is NexportRegistrationFieldType.SelectCheckbox or NexportRegistrationFieldType.SelectDropDown)
                    {
                        if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                            fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(f,
                                nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                        fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(f,
                            nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                    }

                    return fieldModel;
                })
                .ToListAsync();

            return model;
        }

        public async Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(
            Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new NexportAddCustomerRegistrationFieldsModel();

            var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);

            model.RegistrationFields = await availableFields
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Name)
                .SelectAwait(async x =>
                {
                    var fieldModel = x.ToModel<NexportRegistrationFieldModel>();
                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox ||
                        fieldModel.Type == NexportRegistrationFieldType.SelectDropDown)
                    {
                        if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                            fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(x,
                                nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                        fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(x,
                            nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                    }

                    return fieldModel;
                })
                .ToListAsync();

            return model;
        }

        public async Task<NexportAddCustomerRegistrationFieldsModel> PrepareNexportAddCustomerRegistrationFieldsModel(
            Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new NexportAddCustomerRegistrationFieldsModel();

            var availableFields = await _nexportService.GetNexportRegistrationFields(store.Id);
            var customerExistingFields = await _nexportService.GetNexportRegistrationFieldsWithAnswers(customer.Id, store.Id);
            var fields = availableFields.Where(x => customerExistingFields.All(f => f.Id != x.Id));

            model.RegistrationFields = await fields
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Name)
                .SelectAwait(async x =>
                {
                    var fieldModel = x.ToModel<NexportRegistrationFieldModel>();
                    if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox ||
                        fieldModel.Type == NexportRegistrationFieldType.SelectDropDown)
                    {
                        if (fieldModel.Type == NexportRegistrationFieldType.SelectCheckbox)
                            fieldModel.AllowMultipleSelection = await _genericAttributeService.GetAttributeAsync(x,
                                nameof(fieldModel.AllowMultipleSelection), defaultValue: false);

                        fieldModel.DisplayOptionByAscendingOrder = await _genericAttributeService.GetAttributeAsync(x,
                            nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                    }

                    return fieldModel;
                })
                .ToListAsync();

            return model;
        }

        public async Task<NexportCustomerRegistrationFieldAnswerListModel>
            PrepareNexportCustomerRegistrationFieldAnswerListModel(
                NexportCustomerRegistrationFieldAnswerListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerNexportRegistrationFieldAnswers = await _nexportService.GetNexportRegistrationFieldAnswersPagination(searchModel.CustomerId, searchModel.FieldId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new NexportCustomerRegistrationFieldAnswerListModel().PrepareToGridAsync(searchModel, customerNexportRegistrationFieldAnswers, () =>
            {
                return customerNexportRegistrationFieldAnswers.SelectAwait(async answer =>
                {
                    var answerModel = answer.ToModel<NexportCustomerRegistrationFieldAnswerModel>();
                    var registrationField = await _nexportService.GetNexportRegistrationFieldById(answer.FieldId);
                    if (registrationField != null)
                    {
                        if (string.IsNullOrEmpty(registrationField.CustomFieldRender))
                        {
                            if (!string.IsNullOrEmpty(answer.TextValue))
                            {
                                answerModel.FieldValue = answer.TextValue;
                            }
                            else if (answer.NumericValue != null)
                            {
                                answerModel.FieldValue = answer.NumericValue.ToString();
                            }
                            else if (answer.DateTimeValue != null)
                            {
                                answerModel.FieldValue = answer.DateTimeValue.ToString();
                            }
                            else if (answer.BooleanValue != null)
                            {
                                answerModel.FieldValue = answer.BooleanValue.Value ? "True" : "False";
                            }
                            else if (answer.FieldOptionId != null)
                            {
                                var fieldOption =
                                    await _nexportService.GetNexportRegistrationFieldOptionById(answer.FieldOptionId.Value,
                                        answer.FieldId);
                                if (fieldOption != null)
                                {
                                    answerModel.FieldValue = fieldOption.OptionValue;
                                }
                            }
                        }
                        else
                        {
                            var customRender = await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender);

                            if (customRender != null)
                            {
                                var customFieldRenderAnswers = await customRender.GetCustomFieldNamesAndValues(searchModel.CustomerId, registrationField.Id);
                                answerModel.FieldValue = string.Join("; ",
                                    customFieldRenderAnswers
                                        .Select(customAnswer =>
                                            string.IsNullOrWhiteSpace(customAnswer.Value)
                                                ? $"{customAnswer.Key}: N/A"
                                                : $"{customAnswer.Key}: {customAnswer.Value}")
                                        .ToList());
                            }
                        }
                    }

                    return answerModel;
                });
            });

            return model;
        }

        public async Task<NexportCustomerRegistrationFieldWithAnswersListModel>
            PrepareNexportCustomerRegistrationFieldWithAnswersListModel(
                NexportCustomerRegistrationFieldWithAnswersListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerNexportRegistrationFieldsWithAnswers = await _nexportService.GetNexportRegistrationFieldsWithAnswersPagination(searchModel.CustomerId,
                searchModel.StoreId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = new NexportCustomerRegistrationFieldWithAnswersListModel().PrepareToGrid(searchModel, customerNexportRegistrationFieldsWithAnswers, () =>
            {
                return customerNexportRegistrationFieldsWithAnswers.Select(field =>
                {
                    var fieldModel = field.ToModel<NexportCustomerRegistrationFieldWithAnswersModel>();
                    fieldModel.CustomerId = searchModel.CustomerId;
                    fieldModel.FieldType = field.Type.GetDisplayName();
                    fieldModel.NexportCustomProfileFieldKey = field.NexportCustomProfileFieldKey;

                    return fieldModel;
                });
            });

            return model;
        }

        public async Task<NexportCustomerRegistrationFieldAnswersEditModel>
            PrepareNexportCustomerRegistrationFieldAnswersEditModel(Customer customer,
                NexportRegistrationField registrationField)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (registrationField == null)
                throw new ArgumentNullException(nameof(registrationField));

            var currentRegistrationFieldAnswers = await _nexportService.GetNexportRegistrationFieldAnswers(customer.Id, registrationField.Id);

            var registrationFieldModel = registrationField.ToModel<NexportRegistrationFieldModel>();
            await PrepareNexportRegistrationFieldModelAsync(registrationFieldModel, registrationField);

            var model = new NexportCustomerRegistrationFieldAnswersEditModel
            {
                RegistrationField = registrationFieldModel,
                Options = await _nexportService.GetNexportRegistrationFieldOptions(registrationField.Id),
                Answers = currentRegistrationFieldAnswers
            };

            return model;
        }

        public async Task<NexportOrderInvoiceItemListModel> PrepareNexportOrderInvoiceItemListModelAsync(
            NexportOrderInvoiceItemSearchModel searchModel, bool excludeNonApproval = false)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var nexportOrderInvoiceItems =
                await _nexportService.GetNexportOrderInvoiceItems(searchModel.OrderId, excludeNonApproval,
                    searchModel.Page - 1, searchModel.PageSize);

            var model = await new NexportOrderInvoiceItemListModel().PrepareToGridAsync(searchModel,
                nexportOrderInvoiceItems, () =>
                {
                    return nexportOrderInvoiceItems.SelectAwait(async orderInvoiceItem =>
                    {
                        var orderInvoiceItemModel = orderInvoiceItem.ToModel<NexportOrderInvoiceItemModel>();

                        var order = await _orderService.GetOrderByIdAsync(orderInvoiceItemModel.OrderId);
                        var orderItem = await _orderService.GetOrderItemByIdAsync(orderInvoiceItemModel.OrderItemId);
                        var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                        var productMapping = await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId, store.Id) ??
                                             await _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);
                        if (productMapping != null)
                        {
                            orderInvoiceItemModel.ProductName = (await _productService.GetProductByIdAsync(orderItem.ProductId)).Name;
                            orderInvoiceItemModel.NexportProductName = productMapping.NexportProductName;
                            if (productMapping.NexportSyllabusId != null)
                            {
                                orderInvoiceItemModel.NexportSyllabusId = productMapping.NexportSyllabusId.Value;
                                Guid orgId;

                                if (productMapping.NexportSubscriptionOrgId != null)
                                    orgId = productMapping.NexportSubscriptionOrgId.Value;
                                else
                                    orgId = await _genericAttributeService.GetAttributeAsync<Guid?>(store,
                                        // ReSharper disable once PossibleInvalidOperationException
                                        "NexportSubscriptionOrganizationId", store.Id) ?? _nexportSettings.RootOrganizationId.Value;

                                var nexportUserMapping = await _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                                var existingEnrollment = await _nexportService.GetSectionEnrollmentDetailsAsync(
                                    orgId, nexportUserMapping.NexportUserId, productMapping.NexportSyllabusId.Value);
                                if (existingEnrollment != null)
                                {
                                    orderInvoiceItemModel.ExistingEnrollmentId = existingEnrollment.EnrollmentId;
                                    orderInvoiceItemModel.UtcExistingEnrollmentExpirationDate = existingEnrollment.ExpirationDate;
                                }
                            }
                        }

                        return orderInvoiceItemModel;
                    });
                });

            return model;
        }

        public Task<NexportOrderInvoiceItemModel> PrepareNexportOrderInvoiceItemModelAsync(
            NexportOrderInvoiceItemModel model,
            NexportOrderInvoiceItem orderInvoiceItem)
        {
            model ??= orderInvoiceItem.ToModel<NexportOrderInvoiceItemModel>();

            return Task.FromResult(model);
        }

        /// <summary>
        /// Prepare paged store list model
        /// </summary>
        /// <param name="searchModel">Store search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store list model
        /// </returns>
        public virtual async Task<StoreListModel> PrepareStoreListModel(NexportStoreSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get stores
            var stores = await _nexportService.GetAllStoresAsync(storeName: searchModel.SearchStoreName,
                storeUrl: searchModel.SearchStoreUrl, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = new StoreListModel().PrepareToGrid(searchModel, stores, () =>
            {
                //fill in model values from the entity
                return stores.Select(store => store.ToModel<StoreModel>());
            });

            return model;
        }
    
        public async Task<NexportOrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.VendorId = currentVendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            var filterByProductId = product != null && (currentVendor == null || product.VendorId == currentVendor.Id)
                ? searchModel.ProductId : 0;

            //get orders
            var orders = await _orderService.SearchOrdersAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new NexportOrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                //fill in model values from the entity
                return orders.SelectAwait(async order =>
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                    var storeById = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var orderModel = new NexportOrderModel
                    {
                        Id = order.Id,
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.CustomerId,
                        CustomOrderNumber = order.CustomOrderNumber,
                        StoreUrl = storeById?.Url,
                        //convert dates to the user time
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                        StoreName = storeById?.Name ?? "Deleted",
                        OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                        PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                        ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                        OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false)
                    };
                    return orderModel;
                });
            });
            return model;
        }

        public virtual async Task<WholesaleCreateModel> PrepareWholesaleOrderModelAsync()
        {
            var root = _nexportSettings.RootOrganizationId.Value;
            var stores = await _storeService.GetAllStoresAsync();
            var storesList = stores.Select(store => new SelectListItem(store.Name, store.Id.ToString())).ToList();
            var organizations = await _nexportService.FindAllOrganizationsAsync(root);
            var organizationsList = organizations.Select(organizationToListItem).ToList();
            var products = (await stores.SelectAwait(storeToProducts).ToListAsync()).SelectMany(collectionSelector).ToList();
            var paymentMethods = await _paymentPluginManager.LoadActivePluginsAsync();
            var paymentMethodsList = paymentMethods.Select(paymentMethodToListItem).ToList();
            var wholesaleCreateModel = new WholesaleCreateModel
            {
                AvailableOrganizations = organizationsList,
                AvailableStores = storesList,
                AvailableProducts = products,
                AvailablePaymentMethods = paymentMethodsList
            };
            return wholesaleCreateModel;
            SelectListItem organizationToListItem(OrganizationResponseItem organization) => new(organization.ShortName + $" ({organization.Name})", organization.OrgId.ToString());
            IEnumerable<SelectListItem> collectionSelector(IList<NexportProductMapping> productMappings) => productMappings.Select(productToListItem);
            SelectListItem productToListItem(NexportProductMapping mapping) => new(mapping.DisplayName, mapping.NopProductId.ToString());
            async ValueTask<IList<NexportProductMapping>> storeToProducts(Store store) => await _nexportService.GetProductMappingsByStoreId(store.Id);
            SelectListItem paymentMethodToListItem(IPaymentMethod paymentMethod) => new(paymentMethod.PaymentMethodType.ToString(), paymentMethod.ToPluginModel<PaymentMethodModel>().SystemName);
        }

        public async Task<OrderSummaryCartFooterModel> PrepareOrderSummaryCartFooterModel(OrderSummaryCartFooterModel orderSummaryCartFooterModel, Customer customer, Store store)
        {
            if (orderSummaryCartFooterModel == null)
                throw new ArgumentNullException(nameof(orderSummaryCartFooterModel));

            //TODO - JS: api call to check if customer is purchasing agent
            //check if customer is purchasing agent. if so then display dropdown of groups that can be purchased for
            orderSummaryCartFooterModel.IsPurchasingAgent = true;

            if (orderSummaryCartFooterModel.IsPurchasingAgent)
            {
                var selectedGroupId = await _genericAttributeService.GetAttributeAsync<Guid>(customer,"GroupForCustomer",store.Id);

                if (selectedGroupId != Guid.Empty)
                {
                    orderSummaryCartFooterModel.GroupGuid = selectedGroupId;
                }
                else
                {
                    //set groupforcustomer attribute so we don't run into null later
                    await _genericAttributeService.SaveAttributeAsync(customer, $"GroupForCustomer",
                        selectedGroupId, store.Id);
                }

                //TODO - JS: use api call that narrows groups down to this customer instead of all groups
                //prepare groups
                var orgs = await _nexportService.FindAllOrganizationsUnderRootOrganizationAsync();
                orderSummaryCartFooterModel.AvailableGroups = orgs.Select(x=>
                    new SelectListItem(x.Name,$"{x.OrgId}")
                ).ToList();

                //orderSummaryCartFooterModel.AvailableGroups = new List<SelectListItem>
                //{
                //    new SelectListItem("organization 1", "8fc17e71-7cad-4e98-9cad-ee576fb18d55"), new SelectListItem("organization 2", "ed850ffa-c341-4a20-935c-f2afe17601c0"), new SelectListItem("organization 3", "305bb0a8-f586-4f5b-884f-f36917d956fc")
                //};

                // set default value of 0 that displays an empty string as the first item in the dropdown
               // await PrepareDefaultItemAsync(orderSummaryCartFooterModel.AvailableGroups, "None",$"{Guid.Empty}");
            }

            return orderSummaryCartFooterModel;
        }

        /// <summary>
        /// Prepare default item
        /// </summary>
        /// <param name="items">Available items</param>
        /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
        /// <param name="defaultItemValue">Default item value; defaults 0</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, string defaultItemText = null, string defaultItemValue = "0")
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        public virtual async Task<CustomerNexportGroupsModel> PrepareCustomerNexportGroupsModelAsync(int customerId,int? page)
        {
            var pageSize = 5;
            var pageIndex = 0;

            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            //TODO JS: - Using mock api service right now. this may be different for actual implementation
            var groups = (await _nexportService.FindAllOrganizationsUnderRootOrganizationAsync()).Select(x=>new CustomerNexportGroupModel{GroupGuid = x.OrgId, Name=x.Name, ShortName = x.ShortName}).ToList();
           // var groups = (await _nexportService.FindNexportGroupsByCustomerAsync(customerId)).Select(x=>new CustomerNexportGroupModel{GroupGuid = x.OrgId, Name=x.Name, ShortName = x.ShortName}).ToList();
           

            var pagedGroups = new PagedList<CustomerNexportGroupModel>(groups, pageIndex, pageSize);

            var pagerModel = new PagerModel(_localizationService)
            {
                PageSize = pageSize,
                TotalRecords = pagedGroups.TotalCount,
                PageIndex = pageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerNexportGroupsPaged",
                UseRouteLinks = true,
                RouteValues = new CustomerNexportGroupsModel.CustomerNexportGroupsRouteValues { PageNumber = pageIndex }
            };

            var model = new CustomerNexportGroupsModel
            {
                NexportGroups = pagedGroups,
                PagerModel = pagerModel
            };

            return model;
        }


        public virtual async Task<CustomerNexportGroupProductsModel> PrepareCustomerNexportGroupProductsModelAsync(Guid groupId,int? page)
        {
            var pageSize = 5;
            var pageIndex = 0;

            if (page > 0)
            {
                pageIndex = page.Value - 1;
            }

            //TODO JS: - Using mock in service method right now. this may be different for actual implementation
            var products = (await _nexportService.FindProductsByNexportGroupAsync(groupId)).Select(x => new CustomerNexportGroupProductModel { Name = x.Name }).ToList();


            var pagedGroups = new PagedList<CustomerNexportGroupProductModel>(products, pageIndex, pageSize);

            var pagerModel = new PagerModel(_localizationService)
            {
                PageSize = pageSize,
                TotalRecords = pagedGroups.TotalCount,
                PageIndex = pageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerNexportGroupProductsPaged",
                UseRouteLinks = true,
                RouteValues = new CustomerNexportGroupProductsModel.CustomerNexportGroupProductsRouteValues { PageNumber = pageIndex }
            };

            var model = new CustomerNexportGroupProductsModel
            {
                NexportGroupProducts = pagedGroups,
                PagerModel = pagerModel
            };
            var group = await _nexportService.GetOrganizationDetailsAsync(groupId);
            model.GroupModel = new CustomerNexportGroupModel {GroupGuid = group.OrgId, Name = group.Name, ShortName = group.ShortName};

            return model;
        }
    }
}
