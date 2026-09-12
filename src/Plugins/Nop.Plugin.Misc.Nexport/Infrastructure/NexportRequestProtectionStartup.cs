using System.Diagnostics.Metrics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.Nexport.Configuration;
using Nop.Plugin.Misc.Nexport.Controllers;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public partial class NexportRequestProtectionStartup : INopStartup
{
    private const int QueuedRequestLimit = 0;
    private const int RateLimitWindowSegmentCount = 6;
    private static readonly Meter RequestProtectionMeter = new("Nop.Plugin.Misc.Nexport.RequestProtection");
    private static readonly Counter<long> RejectedRequests =
        RequestProtectionMeter.CreateCounter<long>("nexport.request_protection.rejected");
    private static readonly string WorkerName =
        Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var settings = new NexportRequestProtectionConfig();
        configuration.GetSection(nameof(NexportRequestProtectionConfig))
            .Bind(settings, options => options.BindNonPublicProperties = true);
        services.AddSingleton(settings);

        if (!settings.Enabled)
            return;

        Validate(settings);
        var commonSettings = Singleton<AppSettings>.Instance.Get<CommonConfig>();

        services.AddRateLimiter(options =>
        {
            var existingLimiter = options.GlobalLimiter;
            var rateLimiter = CreateRateLimiter(settings);
            var concurrencyLimiter = CreateConcurrencyLimiter(settings);

            options.GlobalLimiter = commonSettings.PermitLimit > 0 && existingLimiter != null
                ? PartitionedRateLimiter.CreateChained(existingLimiter, rateLimiter, concurrencyLimiter)
                : PartitionedRateLimiter.CreateChained(rateLimiter, concurrencyLimiter);
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                var hasRetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata);
                var retryAfter = hasRetryAfter
                    ? Math.Max(1, (int)Math.Ceiling(retryAfterMetadata.TotalSeconds))
                    : 1;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);

                var protection = GetProtection(context.HttpContext, settings);
                if (protection != null)
                {
                    RejectedRequests.Add(1,
                        new KeyValuePair<string, object>("action", protection.Value.Action),
                        new KeyValuePair<string, object>("method", context.HttpContext.Request.Method),
                        new KeyValuePair<string, object>("reason", hasRetryAfter ? "rate" : "concurrency"),
                        new KeyValuePair<string, object>("worker", WorkerName));
                }

                return ValueTask.CompletedTask;
            };
        });
    }

    public void Configure(IApplicationBuilder application)
    {
        var requestProtectionSettings = application.ApplicationServices.GetRequiredService<NexportRequestProtectionConfig>();
        var commonSettings = Singleton<AppSettings>.Instance.Get<CommonConfig>();

        if (requestProtectionSettings.Enabled && commonSettings.PermitLimit <= 0)
            application.UseRateLimiter();
    }

    public int Order => 450;

    private static PartitionedRateLimiter<HttpContext> CreateRateLimiter(NexportRequestProtectionConfig settings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var protection = GetProtection(context, settings);
            if (protection == null)
                return RateLimitPartition.GetNoLimiter("unprotected");

            var clientAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(
                $"{protection.Value.Name}:{clientAddress}",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = protection.Value.Settings.RequestsPerWindow,
                    Window = TimeSpan.FromSeconds(settings.RateLimitWindowSeconds),
                    SegmentsPerWindow = RateLimitWindowSegmentCount,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = QueuedRequestLimit,
                    AutoReplenishment = true
                });
        });
    }

    private static PartitionedRateLimiter<HttpContext> CreateConcurrencyLimiter(NexportRequestProtectionConfig settings)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var protection = GetProtection(context, settings);
            if (protection == null)
                return RateLimitPartition.GetNoLimiter("unprotected");

            return RateLimitPartition.GetConcurrencyLimiter(
                protection.Value.Name,
                _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = protection.Value.Settings.MaxConcurrentRequests,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = QueuedRequestLimit
                });
        });
    }

    private static (string Action, string Name, NexportEndpointProtectionConfig Settings)? GetProtection(
        HttpContext context,
        NexportRequestProtectionConfig settings)
    {
        var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor?.ControllerTypeInfo.AsType() != typeof(NexportCustomerController))
            return null;

        var method = context.Request.Method;
        return (actionDescriptor.ActionName, method) switch
        {
            (nameof(NexportCustomerController.Login), "GET") =>
                ("login", "login-page", settings.LoginPageEndpoint),
            (nameof(NexportCustomerController.Login), "POST") =>
                ("login", "login-submission", settings.LoginSubmissionEndpoint),
            (nameof(NexportCustomerController.Register), "GET") =>
                ("register", "registration-page", settings.RegistrationPageEndpoint),
            (nameof(NexportCustomerController.Register), "POST") =>
                ("register", "registration-submission", settings.RegistrationSubmissionEndpoint),
            _ => null
        };
    }

    private static void Validate(NexportRequestProtectionConfig settings)
    {
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
