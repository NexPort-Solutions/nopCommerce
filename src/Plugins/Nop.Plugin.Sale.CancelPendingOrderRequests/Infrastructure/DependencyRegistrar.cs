using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Controllers;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CancelPendingOrderRequestsPluginService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PendingOrderCancellationRequestService>().As<IPendingOrderCancellationRequestService>().InstancePerLifetimeScope();

            builder.RegisterType<CancelPendingOrderRequestsController>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<PendingOrderCancellationRequestModelFactory>()
                .As<IPendingOrderCancellationRequestModelFactory>().InstancePerLifetimeScope();
        }

        public int Order => 1000;
    }
}