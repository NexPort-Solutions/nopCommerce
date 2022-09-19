using AutoMapper;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;
using Nop.Web.Areas.Admin.Models.Localization;

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
            CreateMap<LocaleStringResource, LocaleResourceModel>();
            
            CreateMap<PendingOrderCancellationRequest, PendingOrderCancellationRequestModel>()
                .ForMember(model => model.CustomerInfo, opts => opts.Ignore());
            CreateMap<PendingOrderCancellationRequestModel, PendingOrderCancellationRequest>();

            CreateMap<PendingOrderCancellationRequestReason, PendingOrderCancellationRequestReasonModel>();
            CreateMap<PendingOrderCancellationRequestReasonModel, PendingOrderCancellationRequestReason>();
        }

        public int Order => 1;
    }
}