using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Controllers;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Data;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CANCEL_PENDING_ORDER_REQUESTS_PLUGIN_CONTEXT_NAME = "nop_object_context_cancel_pending_order_requests_plugin";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CancelPendingOrderRequestsPluginService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PendingOrderCancellationRequestService>().As<IPendingOrderCancellationRequestService>().InstancePerLifetimeScope();

            builder.RegisterType<CancelPendingOrderRequestsController>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<PendingOrderCancellationRequestModelFactory>()
                .As<IPendingOrderCancellationRequestModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PendingOrderCancellationRequest>>()
                .As<IRepository<PendingOrderCancellationRequest>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CANCEL_PENDING_ORDER_REQUESTS_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PendingOrderCancellationRequestReason>>()
                .As<IRepository<PendingOrderCancellationRequestReason>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CANCEL_PENDING_ORDER_REQUESTS_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterPluginDataContext<CancelPendingOrderRequestsObjectContext>(CANCEL_PENDING_ORDER_REQUESTS_PLUGIN_CONTEXT_NAME);

            builder.RegisterType<CancelPendingOrderRequestsObjectContext>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CANCEL_PENDING_ORDER_REQUESTS_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order => 1000;
    }
}