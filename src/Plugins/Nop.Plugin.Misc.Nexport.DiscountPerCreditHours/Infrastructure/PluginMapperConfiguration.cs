using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Areas.Admin.Models.Localization;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure
{
    public class PluginMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public PluginMapperConfiguration()
        {
            CreatePluginMaps();
        }

        protected void CreatePluginMaps()
        {
            CreateMap<LocaleStringResource, LocaleResourceModel>();
        }

        public int Order => 1;
    }
}
