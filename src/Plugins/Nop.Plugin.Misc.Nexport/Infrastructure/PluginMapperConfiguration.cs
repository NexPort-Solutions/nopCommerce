using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public class PluginMapperConfiguration : Profile, IOrderedMapperProfile
{
    public PluginMapperConfiguration() => CreatePluginAdminMaps();

    protected void CreatePluginAdminMaps()
    {
        CreateMap<ProductMapping, ProductMappingModel>()
            .ForMember(model => model.AddGroupMembershipMappingModel, options => options.Ignore())
            .ForMember(model => model.GroupMembershipMappingModels, options => options.Ignore())
            .ForMember(model => model.SupplementalInfoQuestionIds, options => options.Ignore())
            .ForMember(model => model.AvailableSupplementalInfoQuestions, options => options.Ignore());

        CreateMap<ProductMappingModel, ProductMapping>()
            .ForMember(
                entity => entity.IsSynchronized,
                options => options.MapFrom(model => model.IsSynchronized))
            .ForMember(
                entity => entity.UtcLastSynchronizationDate,
                options => options.MapFrom(model => model.UtcLastSynchronizationDate))
            .ForMember(
                entity => entity.AutoRedeem,
                options => options.MapFrom(model => model.AutoRedeem))
            .ForMember(
                entity => entity.SubscriptionOrgId,
                options => options.MapFrom(model => model.SubscriptionOrgId))
            .ForMember(
                entity => entity.SubscriptionOrgName,
                options => options.MapFrom(model => model.SubscriptionOrgName))
            .ForMember(
                entity => entity.SubscriptionOrgShortName,
                options => options.MapFrom(model => model.SubscriptionOrgShortName))
            .ForMember(
                entity => entity.UtcAccessExpirationDate,
                options => options.MapFrom(model => model.UtcAccessExpirationDate))
            .ForMember(
                entity => entity.AccessTimeLimit,
                options => options.MapFrom(model => model.AccessTimeLimit))
            .ForMember(
                entity => entity.IsExtensionProduct,
                options => options.MapFrom(model => model.IsExtensionProduct))
            .ForMember(
                entity => entity.AllowExtension,
                options => options.MapFrom(model => model.AllowExtension))
            .ForMember(
                entity => entity.RenewalWindow,
                options => options.MapFrom(model => model.RenewalWindow))
            .ForMember(
                entity => entity.RenewalDuration,
                options => options.MapFrom(model => model.RenewalDuration))
            .ForMember(
                entity => entity.RenewalCompletionThreshold,
                options => options.MapFrom(model => model.RenewalCompletionThreshold))
            .ForMember(
                entity => entity.RenewalApprovalMethod,
                options => options.MapFrom(model => model.RenewalApprovalMethod))
            .ForMember(
                entity => entity.ExtensionPurchaseLimit,
                options => options.MapFrom(model => model.ExtensionPurchaseLimit))
            .ForAllOtherMembers(options => options.Ignore());

        CreateMap<ProductMapping, ProductMapping>()
            .ForMember(productMapping => productMapping.Id, options => options.Ignore());

        CreateMap<ProductGroupMembershipMapping, ProductGroupMembershipMappingModel>();
        CreateMap<ProductGroupMembershipMappingModel, ProductGroupMembershipMapping>();

        CreateMap<Question, SupplementalInfoQuestionModel>()
            .ForMember(model => model.SupplementalInfoOptionSearchModel, options => options.Ignore());
        CreateMap<SupplementalInfoQuestionModel, Question>();

        CreateMap<Domain.SupplementalInfo.Option, SupplementalInfoOptionModel>()
            .ForMember(model => model.AddGroupMembershipMappingModel, options => options.Ignore())
            .ForMember(model => model.GroupMembershipMappingModels, options => options.Ignore());
        CreateMap<Question, CustomerSupplementalInfoAnsweredQuestionModel>()
            .ForMember(model => model.CustomerId, options => options.Ignore());
        CreateMap<SupplementalInfoOptionModel, Domain.SupplementalInfo.Option>();

        CreateMap<OptionGroupAssociation, SupplementalInfoOptionGroupAssociationModel>();
        CreateMap<SupplementalInfoOptionGroupAssociationModel, OptionGroupAssociation>();

        CreateMap<Domain.SupplementalInfo.Answer, SupplementalInfoAnswerModel>()
            .ForMember(model => model.OptionText, options => options.Ignore())
            .ForMember(model => model.Memberships, options => options.Ignore());

        CreateMap<MappingProduct, MappingProductModel>();
        CreateMap<Product, MappingProductModel>();

        CreateMap<Store, StoreModel>();

        CreateMap<RegistrationField, RegistrationFieldModel>()
            .ForMember(model => model.FieldCategoryName, options => options.Ignore())
            .ForMember(model => model.StoreMappings, options => options.Ignore())
            .ForMember(model => model.AvailableFieldTypes, options => options.Ignore())
            .ForMember(model => model.AvailableFieldCategory, options => options.Ignore())
            .ForMember(model => model.AvailableStores, options => options.Ignore())
            .ForMember(model => model.AvailableCustomFieldRenders, options => options.Ignore())
            .ForMember(model => model.StoreMappingIds, options => options.Ignore())
            .ForMember(model => model.CustomFieldRenderDescription, options => options.Ignore())
            .ForMember(model => model.RegistrationFieldOptionSearchModel, options => options.Ignore());

        CreateMap<RegistrationFieldModel, RegistrationField>();

        CreateMap<Domain.RegistrationField.Option, OptionModel>();
        CreateMap<OptionModel, Domain.RegistrationField.Option>()
            .ForMember(entity => entity.FieldId, options => options.Ignore());

        CreateMap<Domain.RegistrationField.Category, Models.RegistrationField.CategoryModel>();
        CreateMap<Models.RegistrationField.CategoryModel, Domain.RegistrationField.Category>();

        CreateMap<RegistrationField, CustomerWithAnswersModel>()
            .ForMember(model => model.FieldName, options => options.MapFrom(entity => entity.Name))
            .ForMember(model => model.CustomRender, options => options.MapFrom(entity => entity.CustomFieldRender))
            .ForMember(model => model.CustomerId, options => options.Ignore());
        CreateMap<Domain.RegistrationField.Answer, CustomerRegistrationFieldAnswerModel>()
            .ForMember(model => model.FieldValue, options => options.Ignore());

        CreateMap<Core.Domain.Catalog.Category, Models.Category.CategoryModel>();

        CreateMap<OrderInvoiceItem, InvoiceItemModel>()
            .ForMember(model => model.ProductName, options => options.Ignore())
            .ForMember(model => model.NexportProductName, options => options.Ignore())
            .ForMember(model => model.SyllabusId, options => options.Ignore())
            .ForMember(model => model.ExistingEnrollmentId, options => options.Ignore())
            .ForMember(model => model.UtcExistingEnrollmentExpirationDate, options => options.Ignore());
    }

    public int Order => 0;
}
