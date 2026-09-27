using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Configuration;
using UaDetector;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public partial class NexportRequestProtectionStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var settings = new NexportRequestProtectionConfig();
        configuration.GetSection(nameof(NexportRequestProtectionConfig))
            .Bind(settings, options => options.BindNonPublicProperties = true);
        services.AddSingleton(settings);
        // Store-event consumers are discovered even when protection is disabled. Register the guard
        // in all modes, but run its background service only when both switches enable it.
        services.AddSingleton<NexportStoreDomainProtection>();

        if (!settings.Enabled)
            return;

        Validate(settings);
        if (settings.StoreHostGuardMode != NexportStoreHostGuardMode.Disabled)
            // Middleware, store consumers, and the host must share the same snapshot-owning instance.
            services.AddHostedService(provider => provider.GetRequiredService<NexportStoreDomainProtection>());
        services.AddSingleton<NexportRequestProtectionPolicyProvider>();
        services.AddSingleton<NexportRequestAdmission>();
        services.AddBotParser();
        services.AddScoped<INexportEndpointRequestEvaluator, NexportEndpointRequestEvaluator>();
    }

    public void Configure(IApplicationBuilder application)
    {
        var settings = application.ApplicationServices.GetRequiredService<NexportRequestProtectionConfig>();
        if (!settings.Enabled)
            return;

        // Alternate controller routes share the early limiter, without charging admitted requests twice.
        var admission = application.ApplicationServices.GetRequiredService<NexportRequestAdmission>();
        application.Use(next => context => admission.InvokeAsync(context, next, useEndpointMetadata: true));
    }

    public int Order => 450;

    private static void Validate(NexportRequestProtectionConfig settings)
    {
        if (!Enum.IsDefined(settings.StoreHostGuardMode))
            throw new InvalidOperationException($"Invalid {nameof(settings.StoreHostGuardMode)}.");

        if (settings.RateLimitWindowSeconds <= 0)
            throw new InvalidOperationException($"{nameof(settings.RateLimitWindowSeconds)} must be greater than zero.");

        ValidateEndpoint(nameof(settings.LoginPageEndpoint), settings.LoginPageEndpoint);
        ValidateEndpoint(nameof(settings.LoginSubmissionEndpoint), settings.LoginSubmissionEndpoint);
        ValidateEndpoint(nameof(settings.RegistrationPageEndpoint), settings.RegistrationPageEndpoint);
        ValidateEndpoint(nameof(settings.RegistrationSubmissionEndpoint), settings.RegistrationSubmissionEndpoint);
    }

    private static void ValidateEndpoint(string name, NexportEndpointProtectionConfig settings)
    {
        if (settings.RequestsPerWindow <= 0)
            throw new InvalidOperationException($"{name}.{nameof(settings.RequestsPerWindow)} must be greater than zero.");
        if (settings.MaxConcurrentRequests <= 0)
            throw new InvalidOperationException($"{name}.{nameof(settings.MaxConcurrentRequests)} must be greater than zero.");
    }
}