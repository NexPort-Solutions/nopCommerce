using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Configuration;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public sealed class NexportRequestPathProtectionStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void Configure(IApplicationBuilder application)
    {
        var configuration = application.ApplicationServices.GetRequiredService<NexportRequestProtectionConfig>();
        if (!configuration.Enabled)
            return;

        using var scope = application.ApplicationServices.CreateScope();
        var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
        var settings = settingService.LoadSetting<NexportRequestProtectionSettings>(storeId: 0);
        var policyProvider = application.ApplicationServices.GetRequiredService<NexportRequestProtectionPolicyProvider>();
        policyProvider.Update(settings);

        application.UseMiddleware<NexportRequestPathProtectionMiddleware>();
    }

    public int Order => 97;
}