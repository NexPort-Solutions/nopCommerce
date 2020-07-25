using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public class NexportPluginModelFactory : INexportPluginModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IMeasureService _measureService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        private readonly NexportService _nexportService;

        #endregion

        #region Constructor

        public NexportPluginModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager cacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            NexportService nexportService)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _cacheManager = cacheManager;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _orderService = orderService;
            _pictureService = pictureService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _settingModelFactory = settingModelFactory;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _nexportService = nexportService;
        }

        #endregion

        public virtual NexportProductMappingModel PrepareNexportProductMappingModel(NexportProductMapping productMapping, bool isEditable)
        {
            var model = productMapping.ToModel<NexportProductMappingModel>();
            model.Editable = isEditable;
            if (model.StoreId != null)
            {
                model.StoreName = _nexportService.GetStoreName(model.StoreId.Value);
            }

            if (model.NexportSyllabusId != null)
            {
                if (model.Type == NexportProductTypeEnum.Section)
                {
                    var sectionDetails = _nexportService.GetSectionDetails(model.NexportSyllabusId.Value);
                    model.SectionNumber = sectionDetails.SectionNumber;
                    model.UniqueName = sectionDetails.UniqueName;
                }
                else if (model.Type == NexportProductTypeEnum.TrainingPlan)
                {
                    var trainingPlanDetails = _nexportService.GetTrainingPlanDetails(model.NexportSyllabusId.Value);
                    model.UniqueName = trainingPlanDetails.UniqueName;
                }
            }

            model.SupplementalInfoQuestionIds =
                _nexportService.GetNexportSupplementalInfoQuestionMappingsByProductMappingId(productMapping.Id)
                    .Select(x => x.QuestionId).ToList();

            var availableSupplementalInfoQuestions = _nexportService.GetSupplementalInfoQuestionList();
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

        public virtual NexportProductMappingSearchModel PrepareNexportProductMappingSearchModel(NexportProductMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual MapProductToNexportProductListModel PrepareMapProductToNexportProductListModel(NexportProductMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get all all products in the store
            var products = _productService.SearchProducts(showHidden: true,
                //categoryIds: new List<int> { searchModel.SearchCategoryId },
                //manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                //vendorId: searchModel.SearchVendorId,
                //productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            // Prepare grid model
            var model = new MapProductToNexportProductListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<MappingProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        public virtual NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, Guid nexportProductId,
            NexportProductTypeEnum nexportProductType)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var mappings = _nexportService.GetProductMappingsPagination(nexportProductId, nexportProductType,
                searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = new NexportProductMappingListModel().PrepareToGrid(searchModel, mappings, () =>
            {
                return mappings.Select(mapping =>
                {
                    // Fill in model values from the entity
                    var mappingModel = mapping.ToModel<NexportProductMappingModel>();
                    if (mappingModel.StoreId.HasValue)
                    {
                        mappingModel.StoreName = _nexportService.GetStoreName(mappingModel.StoreId.Value);
                    }

                    return mappingModel;
                });
            });

            return model;
        }

        public virtual NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, int nopProductId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var availableStores = _storeService.GetAllStores();
            var mappingCollection = new List<NexportProductMapping>();
            foreach (var store in availableStores)
            {
                var mapping = _nexportService.GetProductMappingByNopProductId(nopProductId, store.Id);
                if (mapping != null)
                {
                    mappingCollection.Add(mapping);
                }
                else
                {
                    mappingCollection.Add(new NexportProductMapping()
                    {
                        NexportCatalogId = Guid.Empty,
                        StoreId = store.Id
                    });
                }
            }

            var defaultMapping = _nexportService.GetProductMappingByNopProductId(nopProductId);
            if (defaultMapping != null)
            {
                mappingCollection.Insert(0, defaultMapping);
            }
            else
            {
                mappingCollection.Insert(0, new NexportProductMapping()
                {
                    NexportCatalogId = Guid.Empty
                });
            }

            var mappings = new PagedList<NexportProductMapping>(mappingCollection, searchModel.Page - 1,
                searchModel.PageSize);

            // Prepare grid model
            var model = new NexportProductMappingListModel().PrepareToGrid(searchModel, mappings, () =>
            {
                return mappings.Select(mapping =>
                {
                    // Fill in model values from the entity
                    var mappingModel = mapping.ToModel<NexportProductMappingModel>();
                    if (mappingModel.StoreId.HasValue)
                    {
                        mappingModel.StoreName = _nexportService.GetStoreName(mappingModel.StoreId.Value);
                    }

                    return mappingModel;
                });
            });

            return model;
        }

        public virtual NexportProductGroupMembershipMappingListModel PrepareNexportProductMappingGroupMembershipListModel(
            NexportProductGroupMembershipMappingSearchModel searchModel, int nexportProductMappingId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var groupMembershipMappings = _nexportService.GetProductGroupMembershipMappingsPagination(nexportProductMappingId,
                searchModel.Page - 1,
                searchModel.PageSize);

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

        public virtual NexportCustomerAdditionalInfoModel PrepareNexportAdditionalInfoModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var model = new NexportCustomerAdditionalInfoModel { CustomerId = customer.Id };
            var nexportUserMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
            if (nexportUserMapping != null)
            {
                model.NexportUserId = nexportUserMapping.NexportUserId;
            }

            model.NexportSupplementalInfoAnswerListSearchModel = new NexportSupplementalInfoAnswerListSearchModel
            {
                CustomerId = customer.Id,
            };

            _baseAdminModelFactory.PrepareStores(model.NexportSupplementalInfoAnswerListSearchModel.AvailableStores);

            model.NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel = new NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel
            {
                CustomerId = customer.Id
            };

            return model;
        }

        public virtual NexportCatalogListModel PrepareNexportCatalogListModel(NexportCatalogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var catalogs = _nexportService.FindAllCatalogs(searchModel.OrgId, searchModel.Page - 1, searchModel.PageSize);

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

        public virtual NexportSyllabusListModel PrepareNexportSyllabusListModel(NexportSyllabusListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var syllabus = _nexportService.FindAllSyllabuses(searchModel.CatalogId, searchModel.Page - 1, searchModel.PageSize);

            // Prepare grid model
            var model = new NexportSyllabusListModel().PrepareToGrid(searchModel, syllabus, () =>
            {
                return syllabus.Select(syllabi =>
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
                        TotalMappings = _nexportService.FindMappingCountPerSyllabi(syllabiId)
                    };

                    if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.Section)
                    {
                        var sectionDetails = _nexportService.GetSectionDetails(syllabi.SyllabusId);
                        if (sectionDetails != null)
                        {
                            syllabiItemModel.UniqueName = sectionDetails.UniqueName;
                            syllabiItemModel.SectionNumber = sectionDetails.SectionNumber;
                        }
                    }
                    else if (syllabi.SyllabusType == GetSyllabiResponseItem.SyllabusTypeEnum.TrainingPlan)
                    {
                        var trainingPlanDetails = _nexportService.GetTrainingPlanDetails(syllabi.SyllabusId);
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

        public virtual NexportLoginModel PrepareNexportLoginModel(bool? checkoutAsGuest)
        {
            return new NexportLoginModel()
            {
                UsernamesEnabled = true,
                RegistrationType = _customerSettings.UserRegistrationType,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage
            };
        }

        public virtual NexportTrainingListModel PrepareNexportTrainingListModel(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var userMapping = _nexportService.FindUserMappingByCustomerId(customer.Id);
            var redemptionOrganizations = _nexportService.FindNexportRedemptionOrganizationsByCustomerId(customer.Id);

            var model = new NexportTrainingListModel();

            if (userMapping != null && redemptionOrganizations != null)
            {
                model.RedemptionOrganizations = redemptionOrganizations;
                model.UserId = userMapping.NexportUserId;
            }

            return model;
        }

        public NexportCustomerSupplementalInfoAnswersModel PrepareNexportCustomerSupplementalInfoAnswersModel(Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new NexportCustomerSupplementalInfoAnswersModel();

            var answers = _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
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

        public NexportCustomerSupplementalInfoAnswerEditModel PrepareNexportCustomerSupplementalInfoAnswersEditModel(
            Customer customer, Store store, NexportSupplementalInfoQuestion question)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var currentAnswers = _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id, question.Id)
                .Select(currentAnswer => new EditSupplementInfoAnswerRequest()
                {
                    AnswerId = currentAnswer.Id,
                    OptionId = currentAnswer.OptionId
                }).ToList();

            var model = new NexportCustomerSupplementalInfoAnswerEditModel
            {
                Question = question,
                Options = _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id, true),
                Answers = currentAnswers
            };

            return model;
        }

        public NexportCustomerSupplementalInfoAnsweredQuestionListModel PrepareNexportSupplementalInfoQuestionListModel(
            NexportCustomerSupplementalInfoAnsweredQuestionListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerSupplementalInfoAnsweredQuestions = _nexportService.GetNexportSupplementalInfoAnsweredQuestionsPagination(searchModel.CustomerId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new NexportCustomerSupplementalInfoAnsweredQuestionListModel().PrepareToGrid(searchModel, customerSupplementalInfoAnsweredQuestions, () =>
            {
                return customerSupplementalInfoAnsweredQuestions.Select(question =>
                {
                    var questionModel = question.ToModel<NexportCustomerSupplementalInfoAnsweredQuestionModel>();

                    return questionModel;
                });
            });

            return model;
        }

        public NexportSupplementalInfoAnswerListModel PrepareNexportSupplementalInfoAnswerListModel(
            NexportSupplementalInfoAnswerListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var customerSupplementalInfoAnswers = _nexportService.GetNexportSupplementalInfoAnswersPagination(
                searchModel.CustomerId,
                searchModel.QuestionId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new NexportSupplementalInfoAnswerListModel().PrepareToGrid(searchModel, customerSupplementalInfoAnswers, () =>
            {
                return customerSupplementalInfoAnswers.Select(answer =>
                {
                    var answerModel = answer.ToModel<NexportSupplementalInfoAnswerModel>();

                    answerModel.StoreName = _storeService.GetStoreById(answer.StoreId).Name;
                    answerModel.OptionText =
                        _nexportService.GetNexportSupplementalInfoOptionById(answer.OptionId).OptionText;
                    answerModel.NexportMemberships = _nexportService
                        .GetNexportSupplementalInfoAnswerMembershipsByAnswerId(answer.Id)
                        .Select(am => am.NexportMembershipId).ToList();

                    return answerModel;
                }).OrderBy(a => a.StoreName);
            });

            return model;
        }

        public virtual NexportSupplementalInfoQuestionSearchModel PrepareNexportSupplementalInfoQuestionSearchModel(
            NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual NexportSupplementalInfoQuestionListModel PrepareNexportSupplementalInfoQuestionListModel(
            NexportSupplementalInfoQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get all available supplemental info questions
            var supplementalInfoQuestions =
                _nexportService.GetAllNexportSupplementalInfoQuestionsPagination(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

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

        public virtual NexportSupplementalInfoQuestionModel PrepareNexportSupplementalInfoQuestionModel(
            NexportSupplementalInfoQuestionModel model, NexportSupplementalInfoQuestion question)
        {
            if (question != null)
            {
                model = model ?? question.ToModel<NexportSupplementalInfoQuestionModel>();

                PrepareNexportSupplementalInfoOptionSearchModel(model.NexportSupplementalInfoOptionSearchModel, question);
            }

            var availableQuestionTypes = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = NexportSupplementalInfoQuestionType.SingleOption.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.SingleOption).ToString(),
                    Selected = true
                },
                new SelectListItem
                {
                    Text = NexportSupplementalInfoQuestionType.MultipleOptions.GetDisplayName(),
                    Value = ((int)NexportSupplementalInfoQuestionType.MultipleOptions).ToString()
                }
            };

            foreach (var type in availableQuestionTypes)
                model.AvailableQuestionTypes.Add(type);

            return model;
        }

        public virtual NexportSupplementalInfoOptionSearchModel PrepareNexportSupplementalInfoOptionSearchModel(
            NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            searchModel.QuestionId = question.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual NexportSupplementalInfoOptionListModel PrepareNexportSupplementalInfoOptionListModel(
            NexportSupplementalInfoOptionSearchModel searchModel, NexportSupplementalInfoQuestion question)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var options = _nexportService.GetNexportSupplementalInfoOptionsByQuestionId(question.Id)
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

        public virtual NexportSupplementalInfoOptionModel PrepareNexportSupplementalInfoOptionModel(
            NexportSupplementalInfoOptionModel model, NexportSupplementalInfoQuestion question,
            NexportSupplementalInfoOption option)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            if (option != null)
            {
                //fill in model values from the entity
                model = model ?? option.ToModel<NexportSupplementalInfoOptionModel>();
            }

            model.QuestionId = question.Id;

            return model;
        }

        public NexportSupplementalInfoOptionGroupAssociationListModel PrepareNexportSupplementalInfoOptionGroupAssociationListModel(
            NexportSupplementalInfoOptionGroupAssociationSearchModel searchModel, int optionId)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var groupAssociations =
                _nexportService.GetNexportSupplementalInfoOptionGroupAssociationsPagination(optionId,
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

        public NexportSupplementalInfoAnswerQuestionModel PrepareNexportSupplementalInfoAnswerQuestionModel(
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

            var answers = _nexportService.GetNexportSupplementalInfoAnswers(customer.Id, store.Id);
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

        public NexportCustomerAdditionalSettingsModel PrepareNexportCustomerAdditionalSettingsModel()
        {
            var model = new NexportCustomerAdditionalSettingsModel();

            return model;
        }

        public NexportRegistrationFieldCategoryListModel PrepareNexportRegistrationFieldCategoryListModel(
            NexportRegistrationFieldCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFields =
                _nexportService.GetNexportRegistrationFieldCategoriesPagination(searchModel.Page - 1, searchModel.PageSize);

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

        public NexportRegistrationFieldCategoryModel PrepareNexportRegistrationFieldCategoryModel(
            NexportRegistrationFieldCategoryModel model, NexportRegistrationFieldCategory registrationFieldCategory)
        {
            if (registrationFieldCategory != null)
            {
                model = model ?? registrationFieldCategory.ToModel<NexportRegistrationFieldCategoryModel>();
            }

            return model;
        }

        public NexportRegistrationFieldOptionSearchModel PrepareNexportRegistrationFieldOptionSearchModel(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (registrationField == null)
                throw new ArgumentNullException(nameof(registrationField));

            searchModel.RegistrationFieldId = registrationField.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public NexportRegistrationFieldListModel PrepareNexportRegistrationFieldListModel(
            NexportRegistrationFieldSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFields =
                _nexportService.GetNexportRegistrationFieldsPagination(searchModel.Page - 1, searchModel.PageSize);

            var model = new NexportRegistrationFieldListModel().PrepareToGrid(searchModel,
                registrationFields, () =>
                {
                    return registrationFields.Select(field =>
                    {
                        var fieldModel = field.ToModel<NexportRegistrationFieldModel>();

                        if (fieldModel.FieldCategoryId.HasValue)
                            fieldModel.FieldCategoryName = _nexportService.GetNexportRegistrationFieldCategoryById(
                                    fieldModel.FieldCategoryId.Value).Title;

                        var storeMappings = _nexportService
                            .GetNexportRegistrationFieldStoreMappings(fieldModel.Id);

                        for (var i = 0; i < storeMappings.Count; i++)
                        {
                            if (i < storeMappings.Count - 1)
                                fieldModel.StoreMappings += $"{_storeService.GetStoreById(storeMappings[i].StoreId).Name}, ";
                            else
                                fieldModel.StoreMappings += $"{_storeService.GetStoreById(storeMappings[i].StoreId).Name}";

                        }

                        return fieldModel;
                    });
                });

            return model;
        }

        public NexportRegistrationFieldModel PrepareNexportRegistrationFieldModel(NexportRegistrationFieldModel model,
            NexportRegistrationField registrationField, bool excludeProperties = false)
        {
            Action<NexportRegistrationFieldLocalizedModel, int> localizedModelConfiguration = null;

            if (registrationField != null)
            {
                model = model ?? registrationField.ToModel<NexportRegistrationFieldModel>();

                model.StoreMappingIds = _nexportService.GetNexportRegistrationFieldStoreMappings(registrationField.Id)
                    .Select(s => s.StoreId).ToList();

                PrepareNexportRegistrationFieldOptionSearchModel(model.RegistrationFieldOptionSearchModel, registrationField);

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(registrationField,
                        entity => entity.Name, languageId,
                        false, false);
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            model.AvailableFieldCategory = _nexportService.GetRegistrationFieldCategoryList();

            var availableStores = _storeService.GetAllStores();
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
                Selected = model.StoreMappingIds.Contains(store.Id)
            }).ToList();

            return model;
        }

        public NexportRegistrationFieldOptionListModel PrepareNexportRegistrationFieldOptionListModel(
            NexportRegistrationFieldOptionSearchModel searchModel, NexportRegistrationField registrationField)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationFieldOptions =
                _nexportService.GetNexportRegistrationFieldOptionsPagination(registrationField.Id,
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

        public NexportRegistrationFieldOptionModel PrepareNexportRegistrationFieldOptionModel(
            NexportRegistrationFieldOptionModel model, NexportRegistrationField registrationField,
            NexportRegistrationFieldOption registrationFieldOption)
        {
            if (registrationFieldOption != null)
            {
                model = model ?? registrationFieldOption.ToModel<NexportRegistrationFieldOptionModel>();
            }

            return model;
        }

        public NexportCustomerRegistrationFieldsModel PrepareNexportCustomerRegistrationFieldsModel(Store store)
        {
            var model = new NexportCustomerRegistrationFieldsModel();

            var availableFields = _nexportService.GetNexportRegistrationFields(store.Id);

            var fieldsWithCategory = availableFields.Where(x => x.FieldCategoryId != null)
                .GroupBy(x =>
                {
                    var fieldCategory = _nexportService.GetNexportRegistrationFieldCategoryById(x.FieldCategoryId.Value);
                    return fieldCategory;
                })
                .ToDictionary(x => x.Key.ToModel<NexportRegistrationFieldCategoryModel>(),
                    x => x
                        .Select(f =>
                            f.ToModel<NexportRegistrationFieldModel>()).ToList())
                .OrderBy(x => x.Key.DisplayOrder)
                .ThenBy(x => x.Key.Title);

            model.RegistrationFieldsWithCategory = fieldsWithCategory.ToDictionary(
                x => x.Key,
                x => x.Value);

            model.RegistrationFieldsWithoutCategory = availableFields
                .Where(x => x.FieldCategoryId == null)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.ToModel<NexportRegistrationFieldModel>())
                .ToList();

            return model;
        }
    }
}
