using Autofac;
using NexportApi.Client;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.Nexport.Controllers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using DefaultLogger = Nop.Plugin.Misc.Nexport.Infrastructure.Logging.DefaultLogger;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            var apiConfiguration = new Configuration();
            builder.RegisterInstance(apiConfiguration).SingleInstance();

            builder.Register(c => new ApiClient(apiConfiguration.BasePath)).As<ISynchronousClient>()
                .InstancePerDependency();
            builder.Register(c => new ApiClient(apiConfiguration.BasePath)).As<IAsynchronousClient>()
                .InstancePerDependency();

            builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerLifetimeScope();

            builder.RegisterType<NexportCustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerLifetimeScope();
            builder.RegisterType<NexportOrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();

            builder.RegisterType<NexportApiService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NexportService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NexportPluginService>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<NexportPluginModelFactory>().As<INexportPluginModelFactory>()
                .InstancePerLifetimeScope();

            builder.RegisterType<NexportIntegrationController>().AsSelf().InstancePerLifetimeScope();
        }

        public int Order => 1000;
    }
}
