using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Sale.PurchaseForCustomer.Factories;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<PurchaseForCustomerService>()
                .As<IPurchaseForCustomerService>().InstancePerLifetimeScope();

            builder.RegisterType<PurchaseForCustomerModelFactory>()
                .As<IPurchaseForCustomerModelFactory>().InstancePerLifetimeScope();

            builder.RegisterType<PurchaseForCustomerPluginService>().AsSelf().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}
