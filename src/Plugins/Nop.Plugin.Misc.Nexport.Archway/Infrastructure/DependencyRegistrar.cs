using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Services;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ArchwayStudentEmployeeRegistrationFieldModelFactory>()
                .As<IArchwayStudentEmployeeRegistrationFieldModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<ArchwayStudentEmployeeRegistrationFieldService>()
                .As<IArchwayStudentEmployeeRegistrationFieldService>().InstancePerLifetimeScope();

            builder.RegisterType<ArchwayPluginService>().AsSelf().InstancePerLifetimeScope();
        }

        public int Order => 999;
    }
}
