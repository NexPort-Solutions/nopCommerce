using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class ArchwayPluginMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public ArchwayPluginMapperConfiguration()
        {
            CreatePluginMaps();
        }

        protected void CreatePluginMaps()
        {
            CreateMap<ArchwayStoreRecordParsingInfo, ArchwayStoreRecordInfo>()
                .ForMember(x => x.Id, opts => opts.Ignore());
        }

        public int Order => 0;
    }
}
