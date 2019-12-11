using Autofac;
using Autofac.Core;
using NexportApi.Client;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Misc.Nexport.Controllers;
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string NEXPORT_PLUGIN_CONTEXT_NAME = "nop_object_context_nexport_plugin";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            var apiConfiguration = new Configuration();

            builder.RegisterInstance(apiConfiguration).SingleInstance();

            builder.RegisterType<NexportCustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerLifetimeScope();
            builder.RegisterType<NexportOrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();

            builder.RegisterType<NexportApiService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NexportService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<NexportPluginService>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<NexportPluginModelFactory>().As<INexportPluginModelFactory>()
                .InstancePerLifetimeScope();

            builder.RegisterType<NexportIntegrationController>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterPluginDataContext<NexportPluginObjectContext>(NEXPORT_PLUGIN_CONTEXT_NAME);

            builder.RegisterType<NexportPluginObjectContext>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportProductMapping>>()
               .As<IRepository<NexportProductMapping>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportProductGroupMembershipMapping>>()
               .As<IRepository<NexportProductGroupMembershipMapping>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportProductStoreMapping>>()
               .As<IRepository<NexportProductStoreMapping>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportUserMapping>>()
               .As<IRepository<NexportUserMapping>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportOrderProcessingQueueItem>>()
               .As<IRepository<NexportOrderProcessingQueueItem>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportOrderInvoiceItem>>()
               .As<IRepository<NexportOrderInvoiceItem>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();

           builder.RegisterType<EfRepository<NexportOrderInvoiceRedemptionQueueItem>>()
               .As<IRepository<NexportOrderInvoiceRedemptionQueueItem>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
               .InstancePerLifetimeScope();
        }

        public int Order => 1000;
    }
}
