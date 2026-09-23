using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nop.Core.Infrastructure;
using Nop.Data;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

/// <summary>
/// Registers the anonymous, early Nexport instance health checkpoint.
/// </summary>
public sealed class NexportHealthCheckStartup : INopStartup
{
    private const string HealthPath = "/healthz";
    private const string HealthCheckName = "nexport-primary-database";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Keep this registration independent of database settings so the endpoint exists while the app is being configured.
        services.AddSingleton<NexportDatabaseHealthCheck>(_ =>
            new NexportDatabaseHealthCheck(CreatePrimarySqlConnection));

        services.AddHealthChecks().AddCheck<NexportDatabaseHealthCheck>(
            HealthCheckName,
            failureStatus: HealthStatus.Unhealthy,
            tags: null,
            timeout: TimeSpan.FromSeconds(5));
    }

    /// <inheritdoc />
    public void Configure(IApplicationBuilder application)
    {
        application.MapWhen(context => context.Request.Path.Equals(HealthPath, StringComparison.OrdinalIgnoreCase), branch =>
        {
            branch.Use(async (context, next) =>
            {
                if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    context.Response.Headers.Allow = "GET, HEAD";
                    return;
                }

                await next();
            });

            branch.UseHealthChecks(HealthPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Name == HealthCheckName
            });
        });
    }

    /// <inheritdoc />
    public int Order => 1; // Run before application middleware that can reject or redirect requests.

    private static DbConnection CreatePrimarySqlConnection()
    {
        try
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            if (dataSettings is null ||
                dataSettings.DataProvider != DataProviderType.SqlServer ||
                string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
            {
                return null;
            }

            return new SqlConnection(dataSettings.ConnectionString);
        }
        catch (Exception)
        {
            // Missing or invalid settings make the endpoint unhealthy, not unavailable.
            return null;
        }
    }
}
