using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string PLUGIN_CONTEXT_NAME = "nop_object_context_archway_fields_plugin";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ArchwayStudentEmployeeRegistrationFieldModelFactory>()
                .As<IArchwayStudentEmployeeRegistrationFieldModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<ArchwayStudentEmployeeRegistrationFieldService>()
                .As<IArchwayStudentEmployeeRegistrationFieldService>().InstancePerLifetimeScope();

            builder.RegisterType<ArchwayPluginService>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterPluginDataContext<PluginObjectContext>(PLUGIN_CONTEXT_NAME);

            builder.RegisterType<PluginObjectContext>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ArchwayStoreRecordInfo>>()
                .As<IRepository<ArchwayStoreRecordInfo>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ArchwayStoreEmployeePosition>>()
                .As<IRepository<ArchwayStoreEmployeePosition>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ArchwayStudentRegistrationFieldAnswer>>()
                .As<IRepository<ArchwayStudentRegistrationFieldAnswer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ArchwayStudentRegistrationFieldKeyMapping>>()
                .As<IRepository<ArchwayStudentRegistrationFieldKeyMapping>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(PLUGIN_CONTEXT_NAME))
                .InstancePerLifetimeScope();
        }

        public int Order => 999;
    }
}
