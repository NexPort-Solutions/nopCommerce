using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Filters;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Infrastructure;

public class PluginStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
        
        // Add action filters
        services.AddMvc(options =>
        {
            options.Filters.Add<DashboardNotificationActionFilter>();
        });

        services.AddScoped<INexportDiscountPerCreditHourModelFactory, NexportDiscountPerCreditHourModelFactory>();
        services.AddScoped<NexportDiscountPerCreditHoursPluginService>();
    }

    public void Configure(IApplicationBuilder application)
    {
        using var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope != null)
        {
            //var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();

            var pluginService = serviceScope.ServiceProvider
                .GetRequiredService<NexportDiscountPerCreditHoursPluginService>();

            Task.Run(() => pluginService.AddOrUpdateResourcesAsync());
        }
    }

    public int Order => 11;
}