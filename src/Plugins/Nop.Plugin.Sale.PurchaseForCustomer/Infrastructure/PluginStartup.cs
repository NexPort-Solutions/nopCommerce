using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Plugin.Sale.PurchaseForCustomer.Services;
using Nop.Plugin.Sale.PurchaseForCustomer.Factories;
using iTextSharp.text;
using System.Threading.Tasks;
using Nop.Plugin.Sale.PurchaseForCustomer.Filters;

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

            services.AddMvc(options =>
            {
                options.Filters.Add<DashboardNotificationActionFilter>();
            });

            services.AddScoped<IPurchaseForCustomerService, PurchaseForCustomerService>();
            services.AddScoped<IPurchaseForCustomerModelFactory, PurchaseForCustomerModelFactory>();
            services.AddScoped<PurchaseForCustomerPluginService>();
            services.AddScoped<IPluginLocalizationService, PluginLocalizationService>();
        }

        public void Configure(IApplicationBuilder application)
        {
            using var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            if (serviceScope != null)
            {
                var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();

                var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentAssemblyVersion != null)
                {
                    var getSettingKeyTask = Task.Run(() => settingService.GetSettingByKeyAsync<string>(PluginDefaults.ASSEMBLY_VERSION_KEY));
                    getSettingKeyTask.Wait();
                    var versionSettingValue = getSettingKeyTask.Result;
                    Version installedAssemblyVersion = null;

                    if (!string.IsNullOrEmpty(versionSettingValue))
                    {
                        installedAssemblyVersion =
                            Version.Parse(versionSettingValue);
                    }

                    if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                    {
                        settingService.SetSettingAsync(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion.ToString());

                        var pluginService =
                            serviceScope.ServiceProvider.GetRequiredService<PurchaseForCustomerPluginService>();

                        Task.Run(() => pluginService.AddOrUpdateResourcesAsync());
                    }

                    
                }
            }
        }

        public int Order => 11;
    }
}