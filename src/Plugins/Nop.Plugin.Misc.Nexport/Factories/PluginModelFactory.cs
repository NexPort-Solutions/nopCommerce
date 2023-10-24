using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.Catalog;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Models.Syllabus;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Stores;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using static Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo.Question;
using static Nop.Plugin.Misc.Nexport.Domain.RegistrationField.RegistrationField;
using OrderModel = Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders.OrderModel;
using DocumentFormat.OpenXml;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using System.Globalization;
using RedemptionType = NexportApi.Model.InvoiceRedemptionResponse.RedemptionTypeEnum;
using SyllabusType = NexportApi.Model.GetSyllabiResponseItem.SyllabusTypeEnum;
using CategoryModel = Nop.Plugin.Misc.Nexport.Models.RegistrationField.CategoryModel;

namespace Nop.Plugin.Misc.Nexport.Factories;

public interface IPluginModelFactory
{
    public IRegistrationFieldModelFactory RegistrationField { get; init; }

    Task<DuplicateProductMappingModel> DuplicateProductMappingModel(Product product);
    Task<CustomerAdditionalInfoModel> AdditionalInfoModel(Customer customer);
    Task<CatalogListModel> CatalogListModel(Guid orgId, CatalogSearchModel model);
    Task<CustomerAdditionalSettingsModel> CustomerAdditionalSettingsModel();
    Task<CustomerSupplementalInfoAnswersEditModel> CustomerSupplementalInfoAnswersEditModel(Customer customer, Store store, Question question);
    Task<CustomerSupplementalInfoAnswersModel> CustomerSupplementalInfoAnswersModel(Customer customer, Store store);
    Task<GroupListModel> GroupListModel(GroupListSearchModel? model);
    Task<GroupProductRedemptionListModel> GroupProductCustomerListModel(GroupProductRedemptionListSearchModel? model, Guid groupId, int productId);
    Task<GroupProductListModel> GroupProductListModel(GroupProductListSearchModel? model, Guid groupId);
    Task<GroupProductListSearchModel> GroupProductListSearchModel(Guid groupId, bool adminView);
    Task<GroupProductRedemptionListSearchModel> GroupProductRedemptionListSearchModel(Guid groupId, int productId);
    Task<LoginModel> LoginModel(bool? checkoutAsGuest);
    Task<InvoiceItemListModel> OrderInvoiceItemListModel(InvoiceItemSearchModel model, bool excludeNonApproval = false);
    Task<InvoiceItemModel> OrderInvoiceItemModel(InvoiceItemModel model, OrderInvoiceItem orderInvoiceItem);
    Task<ProductGroupMembershipMappingListModel> ProductMappingGroupMembershipListModel(ProductGroupMembershipMappingListSearchModel model);
    Task<ProductMappingListModel> ProductMappingListModel(ProductMappingListSearchModel model, int nopProductId);
    ProductMappingListSearchModel ProductMappingListSearchModel(ProductModel productModel);
    Task<ProductMappingModel> ProductMappingModel(ProductMapping productMapping, bool isEditable);
    Task<SupplementalInfoAnswerListModel> SupplementalInfoAnswerListModel(SupplementalInfoAnswerListSearchModel model);
    Task<SupplementalInfoAnswerQuestionModel> SupplementalInfoAnswerQuestionModel(IList<int> questionIds, Customer customer, Store store, string returnUrl);
    Task<SupplementalInfoOptionGroupAssociationListModel> SupplementalInfoOptionGroupAssociationListModel(SupplementalInfoOptionGroupAssociationListSearchModel model);
    Task<SupplementalInfoOptionListModel> SupplementalInfoOptionListModel(SupplementalInfoOptionSearchModel model, Question question);
    Task<SupplementalInfoOptionSearchModel> SupplementalInfoOptionSearchModel(SupplementalInfoOptionSearchModel model, Question question);
    Task<CustomerSupplementalInfoAnsweredQuestionListModel> SupplementalInfoQuestionListModel(CustomerSupplementalInfoAnsweredQuestionListSearchModel model);
    Task<SupplementalInfoQuestionListModel> SupplementalInfoQuestionListModel(SupplementalInfoQuestionSearchModel model);
    Task<SupplementalInfoQuestionModel> SupplementalInfoQuestionModel(SupplementalInfoQuestionModel? model, Question? question);
    Task<SyllabusListModel> SyllabusListModel(SyllabusListSearchModel model);
    Task<TrainingListModel> TrainingListModel(Customer customer);
    Task<Areas.Admin.Models.Orders.OrderListModel> OrderListModel(OrderSearchModel model);
    Task<RedeemProductOrModifyProductRedemptionModel> RedeemProductOrModifyProductRedemptionModel(Guid groupId, int productId, bool adminView, Guid? invoiceItemId = null);
    Task<StoreListModel> StoreListModel(Models.Stores.StoreSearchModel model);
}

public interface IRegistrationFieldModelFactory
{
    // Customer Answers
    Task<AddCustomerAnswersModel> AddCustomerAnswers(Customer customer, Store store);
    Task<AddCustomerAnswersModel> AddCustomerAnswers(Store store);
    Task<CustomerAnswerListModel> CustomerWithAnswersList(int customerId, CustomerAnswersListSearchModel model);
    Task<EditCustomerAnswersModel> EditCustomerAnswers(Customer customer, RegistrationField registrationField);
    Task<CustomerAnswersModel> CustomerAnswers(Store store);
    Task<CustomerWithAnswersListModel> ListCustomerAnswers(ListCustomerAnswersSearchModel model);

    // Category
    Task<Models.RegistrationField.CategoryListModel> CategoryList(Models.RegistrationField.CategorySearchModel model);
    Task<CategoryModel> Category(Domain.RegistrationField.Category category);
    Task<ListModel> List(SearchModel model);
    Task<RegistrationFieldModel> RegistrationFieldModel(RegistrationFieldModel? model, RegistrationField registrationField, bool excludeProperties = false);
    Task<OptionListModel> ListOptionsModel(OptionSearchModel model, RegistrationField registrationField);
    Task<OptionSearchModel> OptionSearchModel(OptionSearchModel model, RegistrationField registrationField);
}

public class RegistrationFieldModelFactory : IRegistrationFieldModelFactory
{
    private readonly IPluginManager<IRegistrationFieldCustomRender> _customRenderPluginManager;
    private readonly IRegistrationFieldService _registrationField;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IStoreService _store;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly ILocalizationService _localization;

    public RegistrationFieldModelFactory(
        IPluginManager<IRegistrationFieldCustomRender> customRenderPluginManager,
        IRegistrationFieldService registrationField,
        ILocalizationService localization,
        IGenericAttributeService genericAttribute,
        IStoreService store,
        ILocalizedModelFactory localizedModelFactory)
    {
        _customRenderPluginManager = customRenderPluginManager;
        _registrationField = registrationField;
        _localization = localization;
        _genericAttribute = genericAttribute;
        _store = store;
        _localizedModelFactory = localizedModelFactory;
    }

    public async Task<Models.RegistrationField.CategoryListModel> CategoryList(Models.RegistrationField.CategorySearchModel model)
    {
        var registrationFields = await _registrationField.GetCategoriesPagination(model.Page - 1, model.PageSize);
        return new Models.RegistrationField.CategoryListModel().PrepareToGrid(
            model,
            registrationFields,
            () => registrationFields.Select(field => field.ToModel<Models.RegistrationField.CategoryModel>()));
    }

    public Task<Models.RegistrationField.CategoryModel> Category(Domain.RegistrationField.Category registrationFieldCategory)
        => Task.FromResult(registrationFieldCategory.ToModel<Models.RegistrationField.CategoryModel>());

    public Task<OptionSearchModel> OptionSearchModel(OptionSearchModel model, RegistrationField registrationField)
    {
        model.RegistrationFieldId = registrationField.Id;
        return Task.FromResult(model);
    }

    public virtual async Task<ListModel> List(SearchModel model)
    {
        var registrationFields = await _registrationField.GetAllPaginated(model.SelectedStoreIds, model.Page - 1, model.PageSize);
        return await new ListModel().PrepareToGridAsync(
            model,
            registrationFields,
            () => registrationFields.SelectAwait(async field =>
            {
                var fieldModel = field.ToModel<RegistrationFieldModel>();
                if (fieldModel.FieldCategoryId is not null)
                {
                    var fieldCategory = await _registrationField.GetCategoryById(fieldModel.FieldCategoryId.Value);
                    fieldModel.FieldCategoryName = fieldCategory?.Title;
                }
                var storeMappings = await _registrationField.GetStoreMappings(fieldModel.Id);
                for (var i = 0; i < storeMappings.Count; i++)
                {
                    var store = await _store.GetStoreByIdAsync(storeMappings[i].StoreId);
                    if (store is null)
                    {
                        continue;
                    }
                    if (i < storeMappings.Count - 1)
                    {
                        fieldModel.StoreMappings += $"{store.Name}, ";
                    }
                    else
                    {
                        fieldModel.StoreMappings += $"{store.Name}";
                    }
                    fieldModel.StoreMappingIds.Add(store.Id);
                }
                if (fieldModel.CustomFieldRender is { } customFieldRender and not "")
                {
                    var customRenderPlugin = await _customRenderPluginManager.LoadPluginBySystemNameAsync(customFieldRender);
                    fieldModel.CustomFieldRenderDescription = customRenderPlugin?.PluginDescriptor.FriendlyName;
                }
                return fieldModel;
            }));
    }

    public virtual async Task<RegistrationFieldModel> RegistrationFieldModel(RegistrationFieldModel? model, RegistrationField registrationField, bool excludeProperties = false)
    {
        model ??= registrationField.ToModel<RegistrationFieldModel>();
        model.AllowMultipleSelection = await _genericAttribute.GetAttributeAsync(registrationField, nameof(model.AllowMultipleSelection), defaultValue: false);
        model.DisplayOptionByAscendingOrder = await _genericAttribute.GetAttributeAsync(registrationField, nameof(model.DisplayOptionByAscendingOrder), defaultValue: false);
        model.StoreMappingIds = (await _registrationField.GetStoreMappings(registrationField.Id))
            .ConvertAll(registrationFieldStoreMapping => registrationFieldStoreMapping.StoreId);
        await OptionSearchModel(model.RegistrationFieldOptionSearchModel, registrationField);
        if (!excludeProperties)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<RegistrationFieldLocalizedModel>(localizedModelConfig);
        }
        model.AvailableFieldCategory = await _registrationField.GetCategoryList();
        model.AvailableStores = (await _store.GetAllStoresAsync())
            .Select(store => new SelectListItem(store.Name, store.Id.ToString(CultureInfo.InvariantCulture), model.StoreMappingIds.Contains(store.Id)))
            .ToList();
        var listItems = (await _registrationField.GetCustomRenders())
            .ConvertAll(fieldCustomRender => new SelectListItem(fieldCustomRender.Name, fieldCustomRender.RenderAssembly));
        //listItems.Insert(0, new SelectListItem("Select", string.Empty));
        model.AvailableCustomFieldRenders = listItems;
        return model;

        async Task localizedModelConfig(RegistrationFieldLocalizedModel locale, int languageId)
            => locale.Name = await _localization.GetLocalizedAsync(registrationField, entity => entity.Name, languageId, false, false);
    }

    public virtual async Task<OptionListModel> ListOptionsModel(OptionSearchModel model, RegistrationField registrationField)
    {
        var registrationFieldOptions = await _registrationField.GetOptionsPaginated(registrationField.Id, model.Page - 1, model.PageSize);
        return new OptionListModel().PrepareToGrid(
            model,
            registrationFieldOptions,
            () => registrationFieldOptions.Select(fieldOption => fieldOption.ToModel<OptionModel>()));
    }

    public async Task<CustomerAnswersModel> CustomerAnswers(Store store)
    {
        var availableFields = await _registrationField.GetAll(store.Id);
        var fieldsWithCategory = (await availableFields.Where(registrationField => registrationField.FieldCategoryId is not null)
            .GroupByAwait(async registrationField => await GetRecord.OrDefault(_registrationField.GetCategoryById, registrationField.FieldCategoryId))
            .ToDictionaryAwaitAsync(
                async asyncGrouping => await Task.Run(() => asyncGrouping.Key.ToModel<CategoryModel>()),
                asyncGrouping => asyncGrouping
                    .SelectAwait(async field =>
                    {
                        var fieldModel = field.ToModel<RegistrationFieldModel>();
                        if (fieldModel.Type is RegistrationFieldType.SelectCheckbox)
                        {
                            fieldModel.AllowMultipleSelection = await _genericAttribute.GetAttributeAsync(field, nameof(fieldModel.AllowMultipleSelection), defaultValue: false);
                        }
                        if (fieldModel.Type is RegistrationFieldType.SelectCheckbox or RegistrationFieldType.SelectDropDown)
                        {
                            fieldModel.DisplayOptionByAscendingOrder = await _genericAttribute.GetAttributeAsync(field, nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                        }
                        return fieldModel;
                    })
                    .OrderBy(registrationFieldModel => registrationFieldModel.DisplayOrder)
                    .ToListAsync()))
            .OrderBy(keyValuePair => keyValuePair.Key.DisplayOrder)
            .ThenBy(keyValuePair => keyValuePair.Key.Title);
        var registrationFieldsWithCategory = fieldsWithCategory.ToDictionary(
            keyValuePair => keyValuePair.Key,
            keyValuePair => keyValuePair.Value);
        var registrationFieldsWithoutCategory = await availableFields
            .Where(registrationField => registrationField.FieldCategoryId is null)
            .OrderBy(registrationField => registrationField.DisplayOrder)
            .SelectAwait(async registrationField =>
            {
                var fieldModel = registrationField.ToModel<RegistrationFieldModel>();
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox)
                {
                    fieldModel.AllowMultipleSelection = await _genericAttribute.GetAttributeAsync(registrationField, nameof(fieldModel.AllowMultipleSelection), defaultValue: false);
                }
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox or RegistrationFieldType.SelectDropDown)
                {
                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttribute.GetAttributeAsync(registrationField, nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                }
                return fieldModel;
            })
            .ToListAsync();
        return new CustomerAnswersModel
        {
            RegistrationFieldsWithCategory = registrationFieldsWithCategory,
            RegistrationFieldsWithoutCategory = registrationFieldsWithoutCategory,
        };
    }

    public async Task<AddCustomerAnswersModel> AddCustomerAnswers(Store store)
    {
        var availableFields = await _registrationField.GetAll(store.Id);
        var registrationFields = await availableFields
            .OrderBy(registrationField => registrationField.Type)
            .ThenBy(registrationField => registrationField.Name)
            .SelectAwait(async field =>
            {
                var fieldModel = field.ToModel<RegistrationFieldModel>();
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox)
                {
                    fieldModel.AllowMultipleSelection = await _genericAttribute.GetAttributeAsync(field, nameof(fieldModel.AllowMultipleSelection), defaultValue: false);
                }
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox or RegistrationFieldType.SelectDropDown)
                {
                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttribute.GetAttributeAsync(field, nameof(fieldModel.DisplayOptionByAscendingOrder), defaultValue: false);
                }
                return fieldModel;
            })
            .ToListAsync();
        return new AddCustomerAnswersModel(registrationFields);
    }

    public async Task<AddCustomerAnswersModel> AddCustomerAnswers(Customer customer, Store store)
    {
        var availableFields = await _registrationField.GetAll(store.Id);
        var customerExistingFields = await _registrationField.GetManyWithAnswers(customer.Id, store.Id);
        var fields = availableFields.Where(availableField => customerExistingFields.All(field => field.Id != availableField.Id));
        var registrationFields = await fields
            .OrderBy(registrationField => registrationField.Type)
            .ThenBy(registrationField => registrationField.Name)
            .SelectAwait(async registrationField =>
            {
                var fieldModel = registrationField.ToModel<RegistrationFieldModel>();
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox)
                {
                    fieldModel.AllowMultipleSelection = await _genericAttribute.GetAttributeAsync<bool>(registrationField, nameof(fieldModel.AllowMultipleSelection));
                }
                if (fieldModel.Type is RegistrationFieldType.SelectCheckbox or RegistrationFieldType.SelectDropDown)
                {
                    fieldModel.DisplayOptionByAscendingOrder = await _genericAttribute.GetAttributeAsync<bool>(registrationField, nameof(fieldModel.DisplayOptionByAscendingOrder));
                }
                return fieldModel;
            })
            .ToListAsync();
        return new AddCustomerAnswersModel(registrationFields);
    }

    public async Task<CustomerAnswerListModel> CustomerWithAnswersList(int customerId, CustomerAnswersListSearchModel model)
    {
        var answers = await _registrationField.GetAnswersPaginated(customerId, model.FieldId, model.Page - 1, model.PageSize);
        return await new CustomerAnswerListModel().PrepareToGridAsync(model, answers, () => answers.SelectAwait(getModel));

        async ValueTask<CustomerRegistrationFieldAnswerModel> getModel(Domain.RegistrationField.Answer answer)
        {
            var answerModel = answer.ToModel<CustomerRegistrationFieldAnswerModel>();
            if (await _registrationField.GetById(answer.FieldId) is not { } registrationField)
            {
                return answerModel;
            }
            answerModel.FieldValue = await IsCustom(registrationField) is { } customRender
                ? await CustomRender(customerId, registrationField, customRender)
                : await BuiltInRender(answer);
            return answerModel;
        }
    }

    private async Task<IRegistrationFieldCustomRender?> IsCustom(RegistrationField registrationField)
        => !string.IsNullOrEmpty(registrationField.CustomFieldRender)
            && await _customRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender) is { } customRender
            ? customRender : null;

    private async Task<string?> BuiltInRender(Domain.RegistrationField.Answer answer)
        => answer switch
        {
            { TextValue: string textValue }
                when !string.IsNullOrEmpty(textValue) => answer.TextValue,
            { NumericValue: int number } => number.ToString(CultureInfo.InvariantCulture),
            { DateTimeValue: DateTime dateTime } => dateTime.ToString(CultureInfo.InvariantCulture),
            { BooleanValue: bool boolean } => boolean ? "True" : "False",
            { FieldOptionId: int fieldOptionId, FieldId: var fieldId }
                when await _registrationField.GetOptionById(fieldOptionId, fieldId) is { OptionValue: var fieldOption } => fieldOption,
            _ => null
        };

    private static async Task<string> CustomRender(int customerId, RegistrationField registrationField, IRegistrationFieldCustomRender customRender)
    {
        var customFieldRenderAnswers = await customRender.GetCustomFieldNamesAndValues(customerId, registrationField.Id);
        var values = customFieldRenderAnswers.Select(answerToRender);
        return string.Join("; ", values);

        static string answerToRender(KeyValuePair<string, string> customAnswer)
            => $"{customAnswer.Key}: {(string.IsNullOrWhiteSpace(customAnswer.Value) ? RENDER_N_A : customAnswer.Value)}";
    }

    public async Task<CustomerWithAnswersListModel> ListCustomerAnswers(ListCustomerAnswersSearchModel model)
    {
        var customerRegistrationFieldsWithAnswers = await _registrationField.GetAllWithAnswersPaginated(
            model.CustomerId,
            model.StoreId,
            pageIndex: model.Page - 1,
            pageSize: model.PageSize);
        return new CustomerWithAnswersListModel().PrepareToGrid(
            model,
            customerRegistrationFieldsWithAnswers,
            () => customerRegistrationFieldsWithAnswers.Select(field =>
            {
                var fieldModel = field.ToModel<CustomerWithAnswersModel>();
                fieldModel.CustomerId = model.CustomerId;
                fieldModel.FieldType = field.Type.GetDisplayName();
                fieldModel.CustomProfileFieldKey = field.CustomProfileFieldKey;
                return fieldModel;
            }));
    }

    public async Task<EditCustomerAnswersModel> EditCustomerAnswers(Customer customer, RegistrationField registrationField)
    {
        var currentRegistrationFieldAnswers = await _registrationField.GetAnswers(customer.Id, registrationField.Id);
        var registrationFieldModel = registrationField.ToModel<RegistrationFieldModel>();
        await RegistrationFieldModel(registrationFieldModel, registrationField);
        return new EditCustomerAnswersModel
        {
            RegistrationField = registrationFieldModel,
            Options = await _registrationField.GetOptions(registrationField.Id),
            Answers = currentRegistrationFieldAnswers,
        };
    }
}

public class PluginModelFactory : IPluginModelFactory
{
    private const string SUBSCRIPTION_ORGANIZATION_ID = "SubscriptionOrganizationId";

    private readonly Settings _settings;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localization;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly IGroupService _group;
    private readonly IOrderService _order;
    private readonly IProductService _product;
    private readonly IStoreService _store;
    private readonly IWorkContext _workContext;
    private readonly CustomerSettings _customerSettings;
    private readonly CaptchaSettings _captchaSettings;
    private readonly IProductGroupMembershipService _productGroupMembership;
    private readonly IProductMappingService _productMapping;
    private readonly IInvoiceService _invoiceService;
    private readonly IAddressService _address;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICatalogService _catalog;
    private readonly ISectionService _section;
    private readonly ITrainingPlanService _trainingPlan;
    private readonly IOrganizationService _organization;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly ICustomerService _customer;
    private readonly IUserMappingService _userMapping;

    public IRegistrationFieldModelFactory RegistrationField { get; init; }

    #region Constructor

    public PluginModelFactory(
        Settings settings,
        IBaseAdminModelFactory baseAdminModelFactory,
        IDateTimeHelper dateTimeHelper,
        ILocalizationService localization,
        IGenericAttributeService genericAttribute,
        IOrderService order,
        IProductService product,
        IStoreService store,
        IWorkContext workContext,
        CustomerSettings customerSettings,
        CaptchaSettings captchaSettings,
        IAddressService address,
        IPriceFormatter priceFormatter,
        IProductGroupMembershipService productGroupMembership,
        IProductMappingService productMapping,
        IInvoiceService invoice,
        ICatalogService catalog,
        ISectionService section,
        ITrainingPlanService trainingPlan,
        IOrganizationService organization,
        ISupplementalInfoService supplementalInfo,
        ICustomerService customer,
        IGroupService group,
        IUserMappingService userMapping,
        IRegistrationFieldModelFactory registrationField)
    {
        _settings = settings;
        _baseAdminModelFactory = baseAdminModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _localization = localization;
        _genericAttribute = genericAttribute;
        _order = order;
        _product = product;
        _store = store;
        _workContext = workContext;
        _customerSettings = customerSettings;
        _captchaSettings = captchaSettings;
        _address = address;
        _priceFormatter = priceFormatter;
        _productGroupMembership = productGroupMembership;
        _productMapping = productMapping;
        _invoiceService = invoice;
        _catalog = catalog;
        _section = section;
        _trainingPlan = trainingPlan;
        _organization = organization;
        _supplementalInfo = supplementalInfo;
        _customer = customer;
        _group = group;
        _userMapping = userMapping;
        RegistrationField = registrationField;
    }

    #endregion Constructor

    public virtual async Task<ProductMappingModel> ProductMappingModel(ProductMapping? productMapping, bool isEditable)
    {
        var model = productMapping.ToModel<ProductMappingModel>();
        var storeName = model.StoreId is { } storeId
            ? await _store.GetStoreNameAsync(storeId)
            : model.StoreName;
        model.Editable = isEditable;
        model.StoreName = storeName;
        if (model.SyllabusId is { } syllabusId)
        {
            if (model.Type is Domain.ProductType.Section && await _section.GetSectionDetails(syllabusId) is { } sectionDetails)
            {
                model.SectionNumber = sectionDetails.SectionNumber;
                model.UniqueName = sectionDetails.UniqueName;
            }
            else if (model.Type is Domain.ProductType.TrainingPlan && await _trainingPlan.GetTrainingPlanDetails(syllabusId) is { UniqueName: var uniqueName })
            {
                model.UniqueName = uniqueName;
            }
        }
        if (model.CatalogId is { } catalogId && await _catalog.GetCatalogDetails(catalogId) is { Name: var name })
        {
            model.CatalogName = name;
        }
        model.SupplementalInfoQuestionIds = (await _supplementalInfo.GetSupplementalInfoQuestionMappings(model.Id)).ConvertAll(mapping => mapping.QuestionId);
        model.AvailableSupplementalInfoQuestions = (await _supplementalInfo.GetSupplementalInfoQuestionList()).Concat(model.AvailableSupplementalInfoQuestions).ToList();
        foreach (var questionItem in model.AvailableSupplementalInfoQuestions)
        {
            questionItem.Selected = int.TryParse(questionItem.Value, out var questionId) && model.SupplementalInfoQuestionIds.Contains(questionId);
        }
        return model;
    }

    public virtual ProductMappingListSearchModel ProductMappingListSearchModel(ProductModel productModel)
    {
        var selectListItems = Enum.GetValues(typeof(Domain.ProductType))
            .OfType<Domain.ProductType>()
            .Select(variant => new SelectListItem(variant.GetDisplayName(), variant.ToString()))
            .ToList();
        return new()
        {
            NopProductId = productModel.Id,
            AvailableProductTypes = selectListItems,
        };
    }

    public virtual async Task<ProductMappingListModel> ProductMappingListModel(ProductMappingListSearchModel model, int nopProductId)
    {
        var mappings = await _productMapping.GetAll(model.SearchProductName, model.ProductType, model.SearchStoreName, nopProductId, model.Page - 1, model.PageSize);
        return await new ProductMappingListModel().PrepareToGridAsync(model, mappings, () => mappings.SelectAwait(getModel));

        async ValueTask<ProductMappingModel> getModel(ProductMapping mapping)
        {
            var mappingModel = mapping.ToModel<ProductMappingModel>();
            if (mappingModel.StoreId is not null)
            {
                mappingModel.StoreName = await _store.GetStoreNameAsync(mappingModel.StoreId.Value);
            }
            if (mappingModel.SyllabusId is not null)
            {
                if (mappingModel.Type is Domain.ProductType.Section)
                {
                    var sectionDetails = await _section.GetSectionDetails(mappingModel.SyllabusId.Value);
                    mappingModel.SectionNumber = sectionDetails?.SectionNumber;
                    mappingModel.UniqueName = sectionDetails?.UniqueName;
                }
                else if (mappingModel.Type is Domain.ProductType.TrainingPlan)
                {
                    var trainingPlanDetails = await _trainingPlan.GetTrainingPlanDetails(mappingModel.SyllabusId.Value);
                    mappingModel.UniqueName = trainingPlanDetails?.UniqueName;
                }
            }
            if (mappingModel.CatalogId is not null)
            {
                var catalogResponseItem = await _catalog.GetCatalogDetails(mappingModel.CatalogId.Value);
                mappingModel.CatalogName = catalogResponseItem?.Name;
            }
            foreach (var groupMembership in await _productMapping.GetAll(mappingModel.Id))
            {
                var groupMembershipModel = groupMembership.ToModel<ProductGroupMembershipMappingModel>();
                mappingModel.GroupMembershipMappingModels.Add(groupMembershipModel);
            }
            return mappingModel;
        }
    }

    public virtual async Task<ProductGroupMembershipMappingListModel> ProductMappingGroupMembershipListModel(
        ProductGroupMembershipMappingListSearchModel model)
    {
        var groupMembershipMappings = await _productGroupMembership.GetProductGroupMembershipMappingsPagination(model.ProductMappingId, model.Page - 1, model.PageSize);
        return new ProductGroupMembershipMappingListModel().PrepareToGrid(
            model,
            groupMembershipMappings,
            () => groupMembershipMappings.Select(mapping => mapping.ToModel<ProductGroupMembershipMappingModel>()));
    }

    public async Task<DuplicateProductMappingModel> DuplicateProductMappingModel(Product product)
    {
        var model = new DuplicateProductMappingModel();
        var defaultMapping = await _productMapping.GetByNopProductId(product.Id);
        if (defaultMapping is null)
        {
            return model;
        }
        var defaultItem = new SelectListItem
        {
            Text = "Default",
            Value = string.Empty,
        };
        model.AvailableStores.Add(defaultItem);
        var availableStores = await _store.GetAllStoresAsync();
        foreach (var store in availableStores)
        {
            var mapping = await _productMapping.GetByNopProductId(product.Id, store.Id);
            var item = new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(CultureInfo.InvariantCulture),
            };
            if (mapping is not null)
            {
                model.AvailableStores.Add(item);
            }
            else
            {
                model.DestinationStores.Add(item);
            }
        }
        return model;
    }

    public virtual async Task<CustomerAdditionalInfoModel> AdditionalInfoModel(Customer customer)
    {
        var model = new CustomerAdditionalInfoModel { CustomerId = customer.Id };
        var userMapping = await _userMapping.FindByCustomerId(customer.Id);
        if (userMapping is not null)
        {
            model.UserId = userMapping.UserId;
        }
        model.SupplementalInfoAnswerListSearchModel = new() { CustomerId = customer.Id };
        await _baseAdminModelFactory.PrepareStoresAsync(model.SupplementalInfoAnswerListSearchModel.AvailableStores);
        model.CustomerSupplementalInfoAnsweredQuestionListSearchModel = new() { CustomerId = customer.Id };
        var availableStores = await _store.GetAllStoresAsync();
        model.ListCustomerAnswersSearchModel = new()
        {
            CustomerId = customer.Id,
            AvailableStores = availableStores.Select(store => new SelectListItem(store.Name, store.Id.ToString(CultureInfo.InvariantCulture))).ToList(),
        };
        model.CustomerAnswersListSearchModel = new() { CustomerId = customer.Id };
        return model;
    }

    public virtual async Task<CatalogListModel> CatalogListModel(Guid orgId, CatalogSearchModel model)
    {
        var catalogs = await _catalog.FindAllCatalogs(orgId, model.Page - 1, model.PageSize);
        return new CatalogListModel().PrepareToGrid(
            model,
            catalogs,
            () => catalogs.Select(catalog => new CatalogResponseItemModel
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
                    AccessTimeLimit = catalog.AccessTimeLimit,
                }));
    }

    public virtual async Task<SyllabusListModel> SyllabusListModel(SyllabusListSearchModel model)
    {
        var syllabus = await _catalog.FindAllSyllabuses(model.CatalogId, model.Page - 1, model.PageSize);
        return await new SyllabusListModel().PrepareToGridAsync(
            model,
            syllabus,
            () => syllabus.SelectAwait(async syllabi =>
            {
                var syllabiItemModel = new SyllabiResponseItemModel
                {
                    CatalogId = model.CatalogId,
                    SyllabusId = syllabi.SyllabusId,
                    Name = syllabi.SyllabusName,
                    Type = syllabi.SyllabusType,
                    ProductId = syllabi.ProductId,
                    TotalMappings = await _productMapping.FindMappingCountForSyllabus(syllabi.SyllabusId),
                };
                if (syllabi.SyllabusType is SyllabusType.Section
                    && await _section.GetSectionDetails(syllabi.SyllabusId) is { } sectionDetails)
                {
                    syllabiItemModel.UniqueName = sectionDetails.UniqueName;
                    syllabiItemModel.SectionNumber = sectionDetails.SectionNumber;
                }
                else if (syllabi.SyllabusType is SyllabusType.TrainingPlan
                    && await _trainingPlan.GetTrainingPlanDetails(syllabi.SyllabusId) is { } trainingPlanDetails)
                {
                    syllabiItemModel.UniqueName = trainingPlanDetails.UniqueName;
                }
                return syllabiItemModel;
            }));
    }

    public virtual Task<LoginModel> LoginModel(bool? checkoutAsGuest)
        => Task.FromResult(new LoginModel
            {
                UsernamesEnabled = false,
                RegistrationType = _customerSettings.UserRegistrationType,
                CheckoutAsGuest = checkoutAsGuest.GetValueOrDefault(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage,
            });

    public virtual async Task<TrainingListModel> TrainingListModel(Customer customer)
    {
        if (await _userMapping.FindByCustomerId(customer.Id) is not { } userMapping)
        {
            return new TrainingListModel();
        }
        var customerOrderInvoices = (await _invoiceService.GetOrderInvoiceItems(userMapping.UserId))
            .Where(orderInvoiceItem => orderInvoiceItem.InvoiceItemId != Guid.Empty)
            .GroupBy(orderInvoiceItem => orderInvoiceItem.RedemptionEnrollmentId)
            .Select(group => group.OrderByDescending(invoice => invoice.UtcDateRedemption).First())
            .OrderByDescending(orderInvoiceItem => orderInvoiceItem.UtcDateRedemption)
            .SelectAwait(async orderInvoice => await _invoiceService.GetInvoiceRedemption(orderInvoice.InvoiceItemId))
            .WhereNotNull()
            .Where(invoiceDetails => invoiceDetails.UtcRedemptionDate is not null)
            .Where(invoiceDetails => invoiceDetails.RedemptionUserId is not null);
        var trainingList = new List<TrainingItemModel>();
        await foreach (var invoiceDetails in customerOrderInvoices)
        {
            var enrollmentDetails = await GetEnrollmentDetails(invoiceDetails.RedemptionType, invoiceDetails.OrganizationId, invoiceDetails.RedemptionUserId!.Value, invoiceDetails.SyllabusId);
            if (!enrollmentDetails.EnrollmentExisted)
            {
                continue;
            }
            var trainingItem = new TrainingItemModel
            {
                Name = invoiceDetails.SyllabusTitle,
                Type = invoiceDetails.RedemptionType ?? RedemptionType.Section,
                UtcStartDate = enrollmentDetails.StartDate,
                UtcExpirationDate = enrollmentDetails.ExpirationDate,
                UtcRedemptionDate = invoiceDetails.UtcRedemptionDate,
                EnrollmentId = invoiceDetails.RedemptionEnrollmentId,
                SyllabusId = invoiceDetails.SyllabusId,
                OrganizationId = invoiceDetails.OrganizationId,
                Status = enrollmentDetails.Status,
            };
            if (!trainingList.Any(item => item.SyllabusId == invoiceDetails.SyllabusId))
            {
                trainingList.Add(trainingItem);
            }
        }
        return new TrainingListModel
        {
            UserId = userMapping.UserId,
            RedemptionOrganizations = await _organization.FindRedemptionOrganizationsByCustomerId(customer.Id),
            Trainings = trainingList
                .GroupBy(trainingItemModel => trainingItemModel.OrganizationId)
                .ToDictionary(group => group.Key, group => group.ToList()),
        };
    }

    private async Task<EnrollmentDetails> GetEnrollmentDetails(RedemptionType? redemptionType, Guid organizationId, Guid redemptionUserId, Guid syllabusId) => redemptionType switch
    {
        null or RedemptionType.Section when await _section.GetSectionEnrollmentDetails(organizationId, redemptionUserId, syllabusId) is { } enrollmentDetails
            => (redemptionType ?? RedemptionType.Section, true, enrollmentDetails.EnrollmentDate, enrollmentDetails.ExpirationDate, enrollmentDetails.Phase),
        RedemptionType.TrainingPlan when await _trainingPlan.GetTrainingPlanEnrollmentDetails(organizationId, redemptionUserId, syllabusId) is { } enrollmentDetails
            => (redemptionType.Value, true, enrollmentDetails.EnrollmentDate, enrollmentDetails.ExpirationDate, enrollmentDetails.Phase),
        _ => (redemptionType ?? RedemptionType.Section, false, null, null, Enums.PhaseEnum.NotStarted),
    };

    public async Task<CustomerSupplementalInfoAnswersModel> CustomerSupplementalInfoAnswersModel(Customer customer, Store store)
    {
        var questionWithAnswersList = new Dictionary<int, Dictionary<int, int>>();
        var answers = await _supplementalInfo.GetSupplementalInfoAnswers(customer.Id, store.Id);
        foreach (var questionId in answers.Select(answer => answer.QuestionId).Distinct())
        {
            var answerWithOptionsList = answers.Where(supplementalInfoAnswer => supplementalInfoAnswer.QuestionId == questionId);
            var answerWithOptionsDictionary = answerWithOptionsList.ToDictionary(answerAndOption => answerAndOption.Id, answerAndOption => answerAndOption.OptionId);
            questionWithAnswersList.Add(questionId, answerWithOptionsDictionary);
        }
        return new CustomerSupplementalInfoAnswersModel(questionWithAnswersList);
    }

    public async Task<CustomerSupplementalInfoAnswersEditModel> CustomerSupplementalInfoAnswersEditModel(Customer customer, Store store, Question question)
    {
        var currentAnswers = (await _supplementalInfo.GetSupplementalInfoAnswers(customer.Id, store.Id, question.Id))
            .ConvertAll(currentAnswer => new EditSupplementInfoAnswerRequest
                {
                    AnswerId = currentAnswer.Id,
                    OptionId = currentAnswer.OptionId,
                });
        return new CustomerSupplementalInfoAnswersEditModel
        {
            Question = question,
            Options = await _supplementalInfo.GetSupplementalInfoOptionsByQuestionId(question.Id, true),
            Answers = currentAnswers,
        };
    }

    public async Task<CustomerSupplementalInfoAnsweredQuestionListModel> SupplementalInfoQuestionListModel(CustomerSupplementalInfoAnsweredQuestionListSearchModel model)
    {
        var customerSupplementalInfoAnsweredQuestions = await _supplementalInfo.GetSupplementalInfoAnsweredQuestionsPagination(model.CustomerId, model.Page - 1, model.PageSize);
        return new CustomerSupplementalInfoAnsweredQuestionListModel().PrepareToGrid(
            model,
            customerSupplementalInfoAnsweredQuestions,
            () => customerSupplementalInfoAnsweredQuestions.Select(question => question.ToModel<CustomerSupplementalInfoAnsweredQuestionModel>()));
    }

    public async Task<SupplementalInfoAnswerListModel> SupplementalInfoAnswerListModel(SupplementalInfoAnswerListSearchModel model)
    {
        var customerSupplementalInfoAnswers = await _supplementalInfo.GetSupplementalInfoAnswersPagination(
            model.CustomerId,
            model.QuestionId,
            model.Page - 1,
            model.PageSize);
        return await new SupplementalInfoAnswerListModel().PrepareToGridAsync(
            model,
            customerSupplementalInfoAnswers,
            () => customerSupplementalInfoAnswers
                .SelectAwait(
                    async answer =>
                    {
                        var answerModel = answer.ToModel<SupplementalInfoAnswerModel>();
                        answerModel.StoreName = (await _store.GetStoreByIdAsync(answer.StoreId)).Name;
                        var supplementalInfoOption = await _supplementalInfo.GetSupplementalInfoOptionById(answer.OptionId);
                        answerModel.OptionText = supplementalInfoOption?.OptionText;
                        answerModel.Memberships = (await _supplementalInfo
                            .GetSupplementalInfoAnswerMembershipsByAnswerId(answer.Id))
                            .ConvertAll(answerMembership => answerMembership.MembershipId);
                        return answerModel;
                    })
                .OrderBy(supplementalInfoAnswerModel => supplementalInfoAnswerModel.StoreName));
    }

    public virtual async Task<SupplementalInfoQuestionListModel> SupplementalInfoQuestionListModel(SupplementalInfoQuestionSearchModel model)
    {
        var supplementalInfoQuestions = await _supplementalInfo.GetAllSupplementalInfoQuestionsPagination(pageIndex: model.Page - 1, pageSize: model.PageSize);
        return new SupplementalInfoQuestionListModel().PrepareToGrid(
            model,
            supplementalInfoQuestions,
            () => supplementalInfoQuestions.Select(question => question.ToModel<SupplementalInfoQuestionModel>()));
    }

    public virtual async Task<SupplementalInfoQuestionModel> SupplementalInfoQuestionModel(SupplementalInfoQuestionModel? model, Question? question)
    {
        if (question is not null)
        {
            model ??= question.ToModel<SupplementalInfoQuestionModel>();
            await SupplementalInfoOptionSearchModel(model.SupplementalInfoOptionSearchModel, question);
        }
        model ??= new();
        var availableQuestionTypes = new List<SelectListItem>
        {
            new(QuestionType.SingleOption.GetDisplayName(), ((int)QuestionType.SingleOption).ToString(CultureInfo.InvariantCulture), true),
            new(QuestionType.MultipleOptions.GetDisplayName(), ((int)QuestionType.MultipleOptions).ToString(CultureInfo.InvariantCulture)),
        };
        model.AvailableQuestionTypes.AddRange(availableQuestionTypes);
        return model;
    }

    public virtual Task<SupplementalInfoOptionSearchModel> SupplementalInfoOptionSearchModel(SupplementalInfoOptionSearchModel model, Question question)
    {
        model.QuestionId = question.Id;
        model.SetGridPageSize();
        return Task.FromResult(model);
    }

    public virtual async Task<SupplementalInfoOptionListModel> SupplementalInfoOptionListModel(SupplementalInfoOptionSearchModel model, Question question)
    {
        var options = (await _supplementalInfo.GetSupplementalInfoOptionsByQuestionId(question.Id))
            .ToPagedList(model);
        return new SupplementalInfoOptionListModel().PrepareToGrid(
            model,
            options,
            () => options.Select(option => option.ToModel<SupplementalInfoOptionModel>()));
    }

    public async Task<SupplementalInfoOptionGroupAssociationListModel> SupplementalInfoOptionGroupAssociationListModel(SupplementalInfoOptionGroupAssociationListSearchModel model)
    {
        var groupAssociations = await _supplementalInfo.GetSupplementalInfoOptionGroupAssociationsPagination(model.OptionId, model.Page - 1, model.PageSize);
        return new SupplementalInfoOptionGroupAssociationListModel().PrepareToGrid(
            model,
            groupAssociations,
            () => groupAssociations.Select(mapping => mapping.ToModel<SupplementalInfoOptionGroupAssociationModel>()));
    }

    public async Task<SupplementalInfoAnswerQuestionModel> SupplementalInfoAnswerQuestionModel(IList<int> questionIds, Customer customer, Store store, string returnUrl)
    {
        var questionWithoutAnswerIds = new List<int>();
        var answeredQuestions = new Dictionary<int, string>();
        var answers = await _supplementalInfo.GetSupplementalInfoAnswers(customer.Id, store.Id);
        var answered = answers.Where(answer => questionIds.Contains(answer.QuestionId)).ToList();
        var questionWithAnswerIds = answered.Count > 0
            ? answered.ConvertAll(answer => answer.QuestionId) : new List<int>();
        questionWithoutAnswerIds.AddRange(questionIds.Except(questionWithAnswerIds));
        foreach (var answer in answered.Where(answer => !answeredQuestions.ContainsKey(answer.Id)))
        {
            answeredQuestions.Add(answer.Id, $"{answer.QuestionId},{answer.OptionId}");
        }
        return new SupplementalInfoAnswerQuestionModel
        {
            QuestionIds = questionIds.Distinct().ToList(),
            QuestionWithoutAnswerIds = questionWithoutAnswerIds,
            ReturnUrl = returnUrl,
        };
    }

    public async Task<CustomerAdditionalSettingsModel> CustomerAdditionalSettingsModel()
    {
        var registrationFieldSearchModel = new SearchModel();
        await _baseAdminModelFactory.PrepareStoresAsync(registrationFieldSearchModel.AvailableStores);
        registrationFieldSearchModel.SetGridPageSize();
        var model = new CustomerAdditionalSettingsModel { RegistrationFieldSearchModel = registrationFieldSearchModel };
        model.RegistrationFieldCategorySearchModel.SetGridPageSize();
        return model;
    }

    public async Task<InvoiceItemListModel> OrderInvoiceItemListModel(InvoiceItemSearchModel model, bool excludeNonApproval = false)
    {
        var invoiceItems = await _invoiceService.GetOrderInvoiceItems(model.Id, excludeNonApproval, model.Page - 1, model.PageSize);
        return await new InvoiceItemListModel().PrepareToGridAsync(
            model,
            invoiceItems,
            () => invoiceItems.SelectAwait(async invoiceItem =>
            {
                var invoiceItemModel = invoiceItem.ToModel<InvoiceItemModel>();
                if (await _order.GetOrderByIdAsync(invoiceItem.OrderId) is not { StoreId: var storeId, CustomerId: var customerId }
                    || await _order.GetOrderItemByIdAsync(invoiceItem.OrderItemId) is not { ProductId: var productId }
                    || await _store.GetStoreByIdAsync(storeId) is not { } store
                    || (await _productMapping.GetByNopProductId(productId, storeId) ?? await _productMapping.GetByNopProductId(productId)) is not { } productMapping)
                {
                    return invoiceItemModel;
                }
                invoiceItemModel.ProductName = (await _product.GetProductByIdAsync(productId)).Name;
                invoiceItemModel.NexportProductName = productMapping.ProductName;
                if (productMapping.SyllabusId is not { } syllabusId)
                {
                    return invoiceItemModel;
                }
                invoiceItemModel.SyllabusId = syllabusId;
                var maybeOrgId = productMapping.SubscriptionOrgId
                    ?? await _genericAttribute.GetAttributeAsync<Guid?>(store, SUBSCRIPTION_ORGANIZATION_ID, storeId)
                    ?? _settings.RootOrganizationId;
                if (await _userMapping.FindByCustomerId(customerId) is { } userMapping
                    && maybeOrgId is { } orgId
                    && await _section.GetSectionEnrollmentDetails(orgId, userMapping.UserId, syllabusId) is { EnrollmentId: var id, ExpirationDate: var date })
                {
                    invoiceItemModel.ExistingEnrollmentId = id;
                    invoiceItemModel.UtcExistingEnrollmentExpirationDate = date;
                }
                return invoiceItemModel;
            }));
    }

    public Task<InvoiceItemModel> OrderInvoiceItemModel(InvoiceItemModel model, OrderInvoiceItem orderInvoiceItem)
    {
        model ??= orderInvoiceItem.ToModel<InvoiceItemModel>();
        return Task.FromResult(model);
    }

    /// <summary>
    /// Prepare paged store list model
    /// </summary>
    /// <param name="model">Store search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the store list model
    /// </returns>
    public virtual async Task<StoreListModel> StoreListModel(Models.Stores.StoreSearchModel model)
    {
        var stores = await _store.GetAllStoresAsync(
            storeName: model.SearchStoreName,
            storeUrl: model.SearchStoreUrl,
            pageIndex: model.Page - 1,
            pageSize: model.PageSize,
            excludeDeleted: true);
        return new StoreListModel().PrepareToGrid(model, stores, () => stores.Select(store => store.ToModel<StoreModel>()));
    }

    public async Task<Areas.Admin.Models.Orders.OrderListModel> OrderListModel(OrderSearchModel model)
    {
        // get parameters to filter orders
        var orderStatusIds = model.OrderStatusIds?.Contains(0) ?? true ? null : model.OrderStatusIds.ToList();
        var paymentStatusIds = model.PaymentStatusIds?.Contains(0) ?? true ? null : model.PaymentStatusIds.ToList();
        var shippingStatusIds = model.ShippingStatusIds?.Contains(0) ?? true ? null : model.ShippingStatusIds.ToList();
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor is not null)
        {
            model.VendorId = currentVendor.Id;
        }
        var sourceTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();
        DateTime? startDateValue = model.StartDate is not null ? _dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, sourceTimeZone) : null;
        DateTime? endDateValue = model.EndDate is not null ? _dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, sourceTimeZone).AddDays(1) : null;
        var product = await _product.GetProductByIdAsync(model.ProductId);
        var filterByProductId = product is not null && (currentVendor is null || product.VendorId == currentVendor.Id) ? model.ProductId : 0;
        // get orders
        var orders = await _order.SearchOrdersAsync(
            storeId: model.StoreId,
            vendorId: model.VendorId,
            productId: filterByProductId,
            warehouseId: model.WarehouseId,
            billingCountryId: model.BillingCountryId,
            paymentMethodSystemName: model.PaymentMethodSystemName,
            createdFromUtc: startDateValue,
            createdToUtc: endDateValue,
            osIds: orderStatusIds,
            psIds: paymentStatusIds,
            ssIds: shippingStatusIds,
            billingPhone: model.BillingPhone,
            billingEmail: model.BillingEmail,
            billingLastName: model.BillingLastName,
            orderNotes: model.OrderNotes,
            pageIndex: model.Page - 1,
            pageSize: model.PageSize);
        return await new Areas.Admin.Models.Orders.OrderListModel().PrepareToGridAsync(
            model,
            orders,
            () => orders.SelectAwait(async order =>
            {
                var billingAddress = await _address.GetAddressByIdAsync(order.BillingAddressId);
                var storeById = await _store.GetStoreByIdAsync(order.StoreId);
                return new OrderModel
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
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    StoreName = storeById?.Name ?? "Deleted",
                    OrderStatus = await _localization.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localization.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localization.GetLocalizedEnumAsync(order.ShippingStatus),
                    OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false),
                };
            }));
    }

    public virtual async Task<GroupListModel> GroupListModel(GroupListSearchModel? model)
    {
        var pagedGroups = _group.GetAllGroupForOrders()
            .WhereNotNull()
            .Select(serializedGroup => JsonConvert.DeserializeObject<GroupModel>(serializedGroup.Value))
            .WhereNotNull()
            .WhereAwait(async group => await _invoiceService.GetInvoiceItemCountForGroupByGuid(group.GroupGuid) is not 0);
        return await new GroupListModel().PrepareToGridAsync(model, await pagedGroups.ToPagedListAsync(model), () => pagedGroups);
    }

    public virtual async Task<GroupProductListModel> GroupProductListModel(GroupProductListSearchModel? model, Guid groupId)
    {
        var products = await _group.GetGroupProductModelForGroupId(groupId);
        var pagedProducts = products.ToPagedList(model);
        return await new GroupProductListModel().PrepareToGridAsync(
            model,
            pagedProducts,
            () => pagedProducts.ToAsyncEnumerable());
    }

    public virtual async Task<GroupProductRedemptionListModel> GroupProductCustomerListModel(GroupProductRedemptionListSearchModel? model, Guid groupId, int productId)
    {
        var redemptions = new List<GroupProductRedemptionModel>();
        var invoiceItems = await _invoiceService.GetInvoiceItemsForGroupIdAndProductIdAndRedeemingUserIdHasValue(groupId, productId);
        foreach (var invoiceItem in invoiceItems)
        {
            var redemptionItem = new GroupProductRedemptionModel { InvoiceItemId = invoiceItem.InvoiceItemId };
            if (invoiceItem.RedeemingUserId is not null && invoiceItem.UtcDateRedemption is null)
            {
                redemptionItem.Status = "Awaiting";
                var redeemer = await _customer.GetCustomerByGuidAsync(invoiceItem.RedeemingUserId.Value);
                redemptionItem.Name = $"{redeemer?.FirstName} {redeemer?.LastName}";
                redemptionItem.Email = redeemer?.Email;
            }
            else if (invoiceItem.RedeemingUserId is not null)
            {
                redemptionItem.Status = "Redeemed";
                var redeemer = await _customer.GetCustomerByGuidAsync(invoiceItem.RedeemingUserId.Value);
                redemptionItem.Name = $"{redeemer?.FirstName} {redeemer?.LastName}";
                redemptionItem.Email = redeemer?.Email;
            }
            redemptions.Add(redemptionItem);
        }
        var pagedRedemptions = redemptions.ToPagedList(model);
        return await new GroupProductRedemptionListModel().PrepareToGridAsync(
            model,
            pagedRedemptions,
            () => pagedRedemptions.ToAsyncEnumerable());
    }

    public async Task<GroupProductListSearchModel> GroupProductListSearchModel(Guid groupId, bool adminView)
    {
        var group = await _group.GetGroupByGroupId(groupId) is { Value: var value } ? JsonConvert.DeserializeObject<GroupModel>(value) : null;
        return new GroupProductListSearchModel
        {
            AdminView = adminView,
            CurrentGroup = group,
        };
    }

    public async Task<GroupProductRedemptionListSearchModel> GroupProductRedemptionListSearchModel(Guid groupId, int productId)
    {
        var group = await _group.GetGroupByGroupId(groupId) is { Value: var value } ? JsonConvert.DeserializeObject<GroupModel>(value) : null;
        return new GroupProductRedemptionListSearchModel
        {
            CurrentGroup = group,
            CurrentProduct = await _product.GetProductByIdAsync(productId),
        };
    }

    public async Task<RedeemProductOrModifyProductRedemptionModel> RedeemProductOrModifyProductRedemptionModel(Guid groupId, int productId, bool adminView, Guid? invoiceItemId = null)
    {
        var group = await _group.GetGroupByGroupId(groupId) is { Value: var value } ? JsonConvert.DeserializeObject<GroupModel>(value) : null;
        var nexportOrderInvoiceItem = invoiceItemId is not null
            ? await _invoiceService.FindOrderInvoiceItemByGuid(invoiceItemId.Value)
            : await _invoiceService.GetFirstAvailableInvoiceItemForGroupIdAndProductId(groupId, productId);
        return new RedeemProductOrModifyProductRedemptionModel
        {
            CurrentGroup = group,
            CurrentProduct = await _product.GetProductByIdAsync(productId),
            HasBeenAssigned = invoiceItemId is not null,
            InvoiceItemId = nexportOrderInvoiceItem?.InvoiceItemId,
        };
    }
}

internal record struct EnrollmentDetails(RedemptionType RedemptionType, bool EnrollmentExisted, DateTime? StartDate, DateTime? ExpirationDate, Enums.PhaseEnum Status)
{
    public static implicit operator (RedemptionType, bool, DateTime?, DateTime?, Enums.PhaseEnum)(EnrollmentDetails value)
        => (value.RedemptionType, value.EnrollmentExisted, value.StartDate, value.ExpirationDate, value.Status);

    public static implicit operator EnrollmentDetails((RedemptionType, bool, DateTime?, DateTime?, Enums.PhaseEnum) value)
        => new(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
}
