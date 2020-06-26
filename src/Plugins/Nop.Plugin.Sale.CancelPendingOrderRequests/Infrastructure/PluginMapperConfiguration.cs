using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
{
    public class PluginMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public PluginMapperConfiguration()
        {
            CreatePluginAdminMaps();
        }

        protected void CreatePluginAdminMaps()
        {
            CreateMap<PendingOrderCancellationRequest, PendingOrderCancellationRequestModel>()
                .ForMember(model => model.CustomerInfo, opts => opts.Ignore());
            CreateMap<PendingOrderCancellationRequestModel, PendingOrderCancellationRequest>();
        }

        public int Order => 1;
    }
}