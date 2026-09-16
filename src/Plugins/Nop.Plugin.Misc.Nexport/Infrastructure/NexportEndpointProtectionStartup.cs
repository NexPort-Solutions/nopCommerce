using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Configuration;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public sealed class NexportEndpointProtectionStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void Configure(IApplicationBuilder application)
    {
        var settings = application.ApplicationServices.GetRequiredService<NexportRequestProtectionConfig>();
        if (settings.Enabled)
            application.UseMiddleware<NexportEndpointProtectionMiddleware>();
    }

    public int Order => 451;
}
