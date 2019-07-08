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
            CreateMap<NexportProductMapping, NexportProductMappingModel>();
        }

        public int Order => 0;
    }
}
