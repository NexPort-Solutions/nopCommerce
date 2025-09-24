using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Internal;
using AutoMapper.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.ReturnRequest;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Plugin.Misc.Nexport.Models.Category;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Plugin.Misc.Nexport.Models.Order;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

/// <summary>
/// Workaround for AutoMapper removal of ForAllOtherMembers
/// </summary>
public static class AutoMapperExtensions
{
    private static readonly PropertyInfo TypeMapActionsProperty = typeof(TypeMapConfiguration).GetProperty("TypeMapActions", BindingFlags.NonPublic | BindingFlags.Instance);

    // not needed in AutoMapper 12.0.1
    private static readonly PropertyInfo DestinationTypeDetailsProperty = typeof(TypeMap).GetProperty("DestinationTypeDetails", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void ForAllOtherMembers<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        var typeMapConfiguration = (TypeMapConfiguration)expression;

        var typeMapActions = (List<Action<TypeMap>>)TypeMapActionsProperty.GetValue(typeMapConfiguration);

        typeMapActions?.Add(typeMap =>
        {
            var destinationTypeDetails = typeMap.DestinationTypeDetails;

            if (destinationTypeDetails == null)
                return;
            foreach (var accessor in destinationTypeDetails.WriteAccessors.Where(m =>
                         typeMapConfiguration.GetDestinationMemberConfiguration(m) == null))
            {
                expression.ForMember(accessor.Name, memberOptions);
            }
        });
    }
}

public class NexportPluginMapperConfiguration : Profile, IOrderedMapperProfile
{
    public NexportPluginMapperConfiguration()
    {
        CreateNexportPluginAdminMaps();
    }

    protected void CreateNexportPluginAdminMaps()
    {
        CreateMap<NexportProductMapping, NexportProductMappingModel>()
            .ForMember(model => model.AddGroupMembershipMappingModel, opts => opts.Ignore())
            .ForMember(model => model.GroupMembershipMappingModels, opts => opts.Ignore())
            .ForMember(model => model.SupplementalInfoQuestionIds, opts => opts.Ignore())
            .ForMember(model => model.AvailableSupplementalInfoQuestions, opts => opts.Ignore());

        CreateMap<NexportProductMappingModel, NexportProductMapping>()
            .ForMember(entity => entity.IsSynchronized,
                opts => opts.MapFrom(model => model.IsSynchronized))
            .ForMember(entity => entity.UtcLastSynchronizationDate,
                opts => opts.MapFrom(model => model.UtcLastSynchronizationDate))
            .ForMember(entity => entity.AutoRedeem,
                opts => opts.MapFrom(model => model.AutoRedeem))
            .ForMember(entity => entity.AllowPurchaseWithExistingEnrollment,
                opts => opts.MapFrom(model => model.AllowPurchaseWithExistingEnrollment))
            .ForMember(entity => entity.NexportSubscriptionOrgId,
                opts => opts.MapFrom(model => model.NexportSubscriptionOrgId))
            .ForMember(entity => entity.NexportSubscriptionOrgName,
                opts => opts.MapFrom(model => model.NexportSubscriptionOrgName))
            .ForMember(entity => entity.NexportSubscriptionOrgShortName,
                opts => opts.MapFrom(model => model.NexportSubscriptionOrgShortName))
            .ForMember(entity => entity.UtcAccessExpirationDate,
                opts => opts.MapFrom(model => model.UtcAccessExpirationDate))
            .ForMember(entity => entity.AccessTimeLimit,
                opts => opts.MapFrom(model => model.AccessTimeLimit))
            .ForMember(entity => entity.IsExtensionProduct,
                opts => opts.MapFrom(model => model.IsExtensionProduct))
            .ForMember(entity => entity.AllowExtension,
                opts => opts.MapFrom(model => model.AllowExtension))
            .ForMember(entity => entity.RenewalWindow,
                opts => opts.MapFrom(model => model.RenewalWindow))
            .ForMember(entity => entity.RenewalDuration,
                opts => opts.MapFrom(model => model.RenewalDuration))
            .ForMember(entity => entity.RenewalCompletionThreshold,
                opts => opts.MapFrom(model => model.RenewalCompletionThreshold))
            .ForMember(entity => entity.RenewalApprovalMethod,
                opts => opts.MapFrom(model => model.RenewalApprovalMethod))
            .ForMember(entity => entity.ExtensionPurchaseLimit,
                opts => opts.MapFrom(model => model.ExtensionPurchaseLimit))
            .ForAllOtherMembers(opts => opts.Ignore());

        CreateMap<NexportProductMapping, NexportProductMapping>()
            .ForMember(x => x.Id, opts => opts.Ignore());

        CreateMap<NexportProductGroupMembershipMapping, NexportProductGroupMembershipMappingModel>();
        CreateMap<NexportProductGroupMembershipMappingModel, NexportProductGroupMembershipMapping>();

        CreateMap<NexportSupplementalInfoQuestion, NexportSupplementalInfoQuestionModel>()
            .ForMember(model => model.NexportSupplementalInfoOptionSearchModel, opts => opts.Ignore());
        CreateMap<NexportSupplementalInfoQuestionModel, NexportSupplementalInfoQuestion>();

        CreateMap<NexportSupplementalInfoOption, NexportSupplementalInfoOptionModel>()
            .ForMember(model => model.AddGroupMembershipMappingModel, opts => opts.Ignore())
            .ForMember(model => model.GroupMembershipMappingModels, opts => opts.Ignore());
        CreateMap<NexportSupplementalInfoQuestion, NexportCustomerSupplementalInfoAnsweredQuestionModel>()
            .ForMember(model => model.CustomerId, opts => opts.Ignore());
        CreateMap<NexportSupplementalInfoOptionModel, NexportSupplementalInfoOption>();

        CreateMap<NexportSupplementalInfoOptionGroupAssociation, NexportSupplementalInfoOptionGroupAssociationModel>();
        CreateMap<NexportSupplementalInfoOptionGroupAssociationModel, NexportSupplementalInfoOptionGroupAssociation>();

        CreateMap<NexportSupplementalInfoAnswer, NexportSupplementalInfoAnswerModel>()
            .ForMember(model => model.OptionText, opts => opts.Ignore())
            .ForMember(model => model.NexportMemberships, opts => opts.Ignore());

        CreateMap<MappingProduct, MappingProductModel>();
        CreateMap<Product, MappingProductModel>();

        CreateMap<Store, NexportStoreModel>();

        CreateMap<NexportRegistrationField, NexportRegistrationFieldModel>()
            .ForMember(model => model.FieldCategoryName, opts => opts.Ignore())
            .ForMember(model => model.StoreMappings, opts => opts.Ignore())
            .ForMember(model => model.AvailableFieldTypes, opts => opts.Ignore())
            .ForMember(model => model.AvailableFieldCategory, opts => opts.Ignore())
            .ForMember(model => model.AvailableStores, opts => opts.Ignore())
            .ForMember(model => model.AvailableCustomFieldRenders, opts => opts.Ignore())
            .ForMember(model => model.StoreMappingIds, opts => opts.Ignore())
            .ForMember(model => model.CustomFieldRenderDescription, opts => opts.Ignore())
            .ForMember(model => model.RegistrationFieldOptionSearchModel, opts => opts.Ignore());

        CreateMap<NexportRegistrationFieldModel, NexportRegistrationField>();

        CreateMap<NexportRegistrationFieldOption, NexportRegistrationFieldOptionModel>();
        CreateMap<NexportRegistrationFieldOptionModel, NexportRegistrationFieldOption>()
            .ForMember(entity => entity.FieldId, opts => opts.Ignore());

        CreateMap<NexportRegistrationFieldCategory, NexportRegistrationFieldCategoryModel>();
        CreateMap<NexportRegistrationFieldCategoryModel, NexportRegistrationFieldCategory>();

        CreateMap<NexportRegistrationField, NexportCustomerRegistrationFieldWithAnswersModel>()
            .ForMember(model => model.FieldName, opts => opts.MapFrom(entity => entity.Name))
            .ForMember(model => model.CustomRender, opts => opts.MapFrom(entity => entity.CustomFieldRender))
            .ForMember(model => model.CustomerId, opts => opts.Ignore());
        CreateMap<NexportRegistrationFieldAnswer, NexportCustomerRegistrationFieldAnswerModel>()
            .ForMember(model => model.FieldValue, opts => opts.Ignore());

        CreateMap<Category, NexportCategoryModel>();

        CreateMap<NexportOrderInvoiceItem, NexportOrderInvoiceItemModel>()
            .ForMember(model => model.ProductName, opts => opts.Ignore())
            .ForMember(model => model.NexportProductName, opts => opts.Ignore())
            .ForMember(model => model.NexportSyllabusId, opts => opts.Ignore())
            .ForMember(model => model.ExistingEnrollmentId, opts => opts.Ignore())
            .ForMember(model => model.UtcExistingEnrollmentExpirationDate, opts => opts.Ignore());

        //TODO @js - determine if we still need this
        CreateMap<WholesalePurchasingGroup, NexportGroupModel>()
            .ForMember(model => model.OrganizationId, opts => opts.MapFrom(entity => entity.NexportGroupId))
            .ForMember(model => model.Name, opts => opts.MapFrom(entity => entity.NexportGroupName))
            .ForMember(model => model.ShortName, opts => opts.MapFrom(entity => entity.NexportGroupShortName))
            .ForMember(model => model.ParentId, opts => opts.Ignore())
            .ForMember(model => model.Type, opts => opts.Ignore());

        CreateMap<NexportFundingPool, NexportFundingPoolModel>();
        CreateMap<NexportFundingPoolModel, NexportFundingPool>();

        CreateMap<Product, WholesaleOrderProductModel>()
            .ForMember(model => model.NexportProductMappingId, opts => opts.Ignore());

        CreateMap<NexportRedemptionUnassignmentRequestModel, NexportRedemptionUnassignmentRequest>()
            .ForMember(model => model.Id, opts => opts.MapFrom(entity => entity.Id))
            .ForMember(model => model.InvoiceItemId, opts => opts.MapFrom(entity => entity.InvoiceItemId))
            .ForMember(model => model.CustomerComments, opts => opts.MapFrom(entity => entity.CustomerComments))
            .ForMember(model => model.RequestedByCustomerId,
                opts => opts.MapFrom(entity => entity.RequestedByCustomerId))
            .ForMember(model => model.RequestStatus, opts => opts.MapFrom(entity => entity.RequestStatus))
            .ForMember(model => model.StaffNotes, opts => opts.MapFrom(entity => entity.StaffNotes))
            .ForMember(model => model.UtcCreatedDate, opts => opts.MapFrom(entity => entity.UtcCreatedDate))
            .ForMember(model => model.UtcLastModifiedDate,
                opts => opts.MapFrom(entity => entity.UtcLastModifiedDate));

        CreateMap<NexportRedemptionUnassignmentRequest, NexportRedemptionUnassignmentRequestModel>()
            .ForMember(entity => entity.Id, opts => opts.MapFrom(model => model.Id))
            .ForMember(entity => entity.InvoiceItemId, opts => opts.MapFrom(model => model.InvoiceItemId))
            .ForMember(entity => entity.CustomerComments, opts => opts.MapFrom(model => model.CustomerComments))
            .ForMember(entity => entity.RequestedByCustomerId,
                opts => opts.MapFrom(model => model.RequestedByCustomerId))
            .ForMember(entity => entity.RequestStatus, opts => opts.MapFrom(model => model.RequestStatus))
            .ForMember(entity => entity.StaffNotes, opts => opts.MapFrom(model => model.StaffNotes))
            .ForMember(entity => entity.UtcCreatedDate, opts => opts.MapFrom(model => model.UtcCreatedDate))
            .ForMember(entity => entity.UtcLastModifiedDate,
                opts => opts.MapFrom(model => model.UtcLastModifiedDate));

        CreateMap<NexportRedemptionUnassignmentRequestReason, NexportRedemptionUnassignmentRequestReasonModel>();
        CreateMap<NexportRedemptionUnassignmentRequestReasonModel, NexportRedemptionUnassignmentRequestReason>();

        CreateMap<NexportRedemptionAssignmentApprovalRequest, NexportRedemptionAssignmentApprovalRequestModel>();
        CreateMap<NexportRedemptionAssignmentApprovalRequestModel, NexportRedemptionAssignmentApprovalRequest>();

        CreateMap<ReturnRequest, NexportReturnRequestModel>();
        CreateMap<ReturnRequestModel, NexportReturnRequestModel>();

        CreateMap<NexportRedemptionAuditLog, NexportRedemptionAuditLogModel>();
        CreateMap<NexportRedemptionAuditLogModel, NexportRedemptionAuditLog>();
    }

    public int Order => 0;
}