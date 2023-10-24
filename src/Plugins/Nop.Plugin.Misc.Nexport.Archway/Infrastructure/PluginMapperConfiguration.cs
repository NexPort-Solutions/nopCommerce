using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure;

public class PluginMapperConfiguration : Profile, IOrderedMapperProfile
{
    public PluginMapperConfiguration() => CreatePluginMaps();

    protected void CreatePluginMaps()
    {
        CreateMap<StoreRecordParsingInfo, StoreRecordInfo>()
            .ForMember(storeRecordInfo => storeRecordInfo.StoreNumber, opts => opts.Ignore());
    }

    public int Order => 0;
}
