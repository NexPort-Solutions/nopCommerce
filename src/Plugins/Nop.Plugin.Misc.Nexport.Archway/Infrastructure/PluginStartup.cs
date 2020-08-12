using System;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Misc.Nexport.Archway.Data;
using Nop.Plugin.Misc.Nexport.Archway.Migrations;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add object context
            services.AddDbContext<PluginObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
        }

        private static IServiceProvider CreateFluentMigratorRunnerService()
        {
            var dataSettings = DataSettingsManager.LoadSettings();

            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore().ConfigureRunner(builder =>
                {
                    builder.AddSqlServer()
                        .WithGlobalConnectionString(dataSettings.DataConnectionString)
                        .WithVersionTable(new ArchwayPluginMigrationVersionTable())
                        .ScanIn(typeof(Nop.Plugin.Misc.Nexport.Archway.Migrations.M001_CreatePluginSchemas).Assembly)
                        .For.Migrations()
                        .For.VersionTableMetaData();
                })
                .AddLogging(lb => lb.AddEventSourceLogger())
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Tags = new[] { PluginDefaults.PluginMigrationTag };
                })
                // Build the service provider
                .BuildServiceProvider(false);
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
                using (var serviceScope = migratorRunnerService.CreateScope())
                {
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
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred during database migration process: {ex.Message}", ex);
            }
        }

        public void Configure(IApplicationBuilder application)
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            if (!dataSettings?.IsValid ?? true)
                return;

            ApplyMigration(application);

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
                        serviceScope.ServiceProvider.GetRequiredService<ArchwayPluginService>();

                    pluginService.AddOrUpdateResources();
                }

                var customEnrollmentRouteControl =
                    settingService.GetSettingByKey<bool>(PluginDefaults.CustomEnrollmentRouteControlSettingKey);
                if (customEnrollmentRouteControl)
                {
                    var customEnrollmentRoute = settingService.GetSettingByKey<string>(PluginDefaults.CustomEnrollmentRouteSettingKey);
                    if (!string.IsNullOrWhiteSpace(customEnrollmentRoute))
                    {
                        var options = new RewriteOptions().AddRedirect("cart", customEnrollmentRoute);
                        application.UseRewriter(options);
                    }
                }
            }
        }

        public int Order => 11;
    }
}