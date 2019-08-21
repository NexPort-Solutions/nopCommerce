using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Models;

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
                .ForMember(entity => entity.AddGroupMembershipMappingModel, opt => opt.Ignore())
                .ForMember(entity => entity.GroupMembershipMappingModels, opt => opt.Ignore());

            CreateMap<NexportProductMappingModel, NexportProductMapping>()
                .ForAllMembers( opt => opt.Ignore());
            CreateMap<NexportProductMappingModel, NexportProductMapping>()
                .ForMember(model => model.AutoRedeem, opt => opt.MapFrom(s => s.AutoRedeem))
                .ForMember(model => model.NexportSubscriptionOrgId, opt => opt.MapFrom(s => s.NexportSubscriptionOrgId))
                .ForMember(model => model.NexportSubscriptionOrgName, opt => opt.MapFrom(s => s.NexportSubscriptionOrgName))
                .ForMember(model => model.NexportSubscriptionOrgShortName, opt => opt.MapFrom(s => s.NexportSubscriptionOrgShortName))
                .ForMember(model => model.UtcAccessExpirationDate, opt => opt.MapFrom(s => s.UtcAccessExpirationDate))
                .ForMember(model => model.AccessTimeLimit, opt => opt.MapFrom(s => s.AccessTimeLimit));

            CreateMap<NexportProductGroupMembershipMapping, NexportProductGroupMembershipMappingModel>();
            CreateMap<NexportProductGroupMembershipMappingModel, NexportProductGroupMembershipMapping>();
        }

        public int Order => 0;
    }
}
