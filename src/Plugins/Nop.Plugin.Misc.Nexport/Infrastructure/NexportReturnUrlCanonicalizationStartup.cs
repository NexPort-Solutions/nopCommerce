using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public sealed class NexportReturnUrlCanonicalizationStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INexportNavigationContextService, NexportNavigationContextService>();
        services.AddTransient<NexportReturnUrlCanonicalizationMiddleware>();
    }

    public void Configure(IApplicationBuilder application)
    {
        application.UseMiddleware<NexportReturnUrlCanonicalizationMiddleware>();
    }

    public int Order => 452;
}