using AutoMapper;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Web.Areas.Admin.Models.Localization;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Infrastructure;

public class PurchaseForCustomerPluginMapperConfiguration : Profile, IOrderedMapperProfile
{

    public PurchaseForCustomerPluginMapperConfiguration()
    {
        CreatePluginMaps();
    }

    protected void CreatePluginMaps()
    {

        CreateMap<LocaleStringResource, LocaleResourceModel>();
    }

    public int Order => 0;
}