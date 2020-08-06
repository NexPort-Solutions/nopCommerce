using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;

namespace Nop.Plugin.Sale.PurchaseForCustomer.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
        }

        public void Configure(IApplicationBuilder application)
        {
            using (var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();

                var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var versionSettingValue = settingService.GetSettingByKey<string>(PluginDefaults.ASSEMBLY_VERSION_KEY);
                Version installedAssemblyVersion = null;

                if (!string.IsNullOrEmpty(versionSettingValue))
                {
                    installedAssemblyVersion =
                        Version.Parse(versionSettingValue);
                }

                if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                {
                    settingService.SetSetting(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion.ToString());

                    var pluginService =
                        serviceScope.ServiceProvider.GetRequiredService<PurchaseForCustomerPluginService>();

                    pluginService.AddOrUpdateResources();
                }
            }
        }

        public int Order => 11;
    }
}