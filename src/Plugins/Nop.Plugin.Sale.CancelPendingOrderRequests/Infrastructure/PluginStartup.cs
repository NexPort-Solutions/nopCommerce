using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Migrations;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Infrastructure
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

        public static IServiceProvider CreateFluentMigratorRunnerService()
        {
            var dataSettings = DataSettingsManager.LoadSettings();

            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore().ConfigureRunner(builder =>
                {
                    builder.AddSqlServer()
                        .WithGlobalConnectionString(dataSettings.ConnectionString)
                        .WithVersionTable(new CancelPendingOrderRequestsPluginMigrationVersionTable())
                        .ScanIn(typeof(Nop.Plugin.Sale.CancelPendingOrderRequests.Migrations.M001_CreatePluginSchemas).Assembly)
                        .For.Migrations()
                        .For.VersionTableMetaData();
                })
                .AddLogging(lb => lb.AddEventSourceLogger())
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Tags = new[] { "CancelPendingOrderRequestPluginMigration" };
                })
                // Build the service provider
                .BuildServiceProvider(false);
        }

        public void Configure(IApplicationBuilder application)
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            if (!dataSettings?.IsValid ?? true)
                return;

            ApplyMigration(application);

            using var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();

            var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var versionSettingValue = settingService.GetSettingByKey<string>(PluginDefaults.ASSEMBLY_VERSION_KEY);
            Version installedAssemblyVersion = null;

            if (!string.IsNullOrEmpty(versionSettingValue))
            {
                installedAssemblyVersion = Version.Parse(versionSettingValue);
            }

            if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
            {
                settingService.SetSetting(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion.ToString());

                var nexportPluginService =
                    serviceScope.ServiceProvider.GetRequiredService<CancelPendingOrderRequestsPluginService>();

                nexportPluginService.AddActivityLogTypes();
                nexportPluginService.AddMessageTemplates();
                nexportPluginService.AddOrUpdateResources();
            }
        }

        /// <summary>
        /// Apply migrations
        /// </summary>
        /// <param name="application"></param>
        private void ApplyMigration(IApplicationBuilder application)
        {
            var logger = EngineContext.Current.Resolve<ILogger>();

            try
            {
                var migratorRunnerService = CreateFluentMigratorRunnerService();
                using var serviceScope = migratorRunnerService.CreateScope();
                var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                try
                {
                    runner.MigrateUp();
                }
                catch (MissingMigrationsException)
                {
                    // ignored
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred during database migration process: {ex.Message}", ex);
            }
        }

        public int Order => 12;
    }
}