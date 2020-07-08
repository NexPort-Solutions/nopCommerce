using AutoMapper;

using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Models;
using Nop.Plugin.Misc.Nexport.Models.ProductMappings;
using Nop.Plugin.Misc.Nexport.Models.Stores;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
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
                .ForAllOtherMembers(opts => opts.Ignore());

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


            //CreateMap<NexportSupplementalInfoOptionModel, NexportSupplementalInfoOption>()
            //    .ForAllMembers(opts => opts.Ignore());
            //CreateMap<NexportSupplementalInfoOptionModel, NexportSupplementalInfoOption>()
            //    .ForMember(model => model.OptionText, opts =>
            //        opts.MapFrom(model => model.OptionText))
            //    .ForMember(model => model.QuestionId, opts =>
            //        opts.MapFrom(model => model.QuestionId));

            CreateMap<NexportSupplementalInfoOptionGroupAssociation, NexportSupplementalInfoOptionGroupAssociationModel>();
            CreateMap<NexportSupplementalInfoOptionGroupAssociationModel, NexportSupplementalInfoOptionGroupAssociation>();

            CreateMap<NexportSupplementalInfoAnswer, NexportSupplementalInfoAnswerModel>()
                .ForMember(model => model.OptionText, opts => opts.Ignore())
                .ForMember(model => model.NexportMemberships, opts => opts.Ignore());

            CreateMap<MappingProduct, MappingProductModel>();
            CreateMap<Product, MappingProductModel>();

            CreateMap<Store, NexportStoreModel>();
        }

        public int Order => 0;
    }
}
