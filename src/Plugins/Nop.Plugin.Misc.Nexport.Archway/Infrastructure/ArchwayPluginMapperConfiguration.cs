using AutoMapper;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Web.Areas.Admin.Models.Localization;

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
            CreateMap<LocaleStringResource, LocaleResourceModel>();

            CreateMap<ArchwayStoreRecordParsingInfo, ArchwayStoreRecordInfo>()
                .ForMember(x => x.Id, opts => opts.Ignore());
        }

        public int Order => 0;
    }
}
