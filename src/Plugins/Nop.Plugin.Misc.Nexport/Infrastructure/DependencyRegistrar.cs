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
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using DefaultLogger = Nop.Plugin.Misc.Nexport.Infrastructure.Logging.DefaultLogger;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string NEXPORT_PLUGIN_CONTEXT_NAME = "nop_object_context_nexport_plugin";

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

            builder.RegisterType<EfRepository<NexportSupplementalInfoQuestion>>()
                .As<IRepository<NexportSupplementalInfoQuestion>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoQuestionMapping>>()
                .As<IRepository<NexportSupplementalInfoQuestionMapping>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoOption>>()
                .As<IRepository<NexportSupplementalInfoOption>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoOptionGroupAssociation>>()
                .As<IRepository<NexportSupplementalInfoOptionGroupAssociation>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoAnswer>>()
                .As<IRepository<NexportSupplementalInfoAnswer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoAnswerMembership>>()
                .As<IRepository<NexportSupplementalInfoAnswerMembership>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRequiredSupplementalInfo>>()
                .As<IRepository<NexportRequiredSupplementalInfo>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportSupplementalInfoAnswerProcessingQueueItem>>()
                .As<IRepository<NexportSupplementalInfoAnswerProcessingQueueItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportGroupMembershipRemovalQueueItem>>()
                .As<IRepository<NexportGroupMembershipRemovalQueueItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationField>>()
                .As<IRepository<NexportRegistrationField>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationFieldOption>>()
                .As<IRepository<NexportRegistrationFieldOption>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationFieldCategory>>()
                .As<IRepository<NexportRegistrationFieldCategory>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationFieldStoreMapping>>()
                .As<IRepository<NexportRegistrationFieldStoreMapping>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationFieldAnswer>>()
                .As<IRepository<NexportRegistrationFieldAnswer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NexportRegistrationFieldSynchronizationQueueItem>>()
                .As<IRepository<NexportRegistrationFieldSynchronizationQueueItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(NEXPORT_PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order => 1000;
    }
}
