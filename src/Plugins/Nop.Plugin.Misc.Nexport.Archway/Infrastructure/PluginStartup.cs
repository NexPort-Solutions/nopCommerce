using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Plugin.Misc.Nexport.Archway.Data;
using Nop.Plugin.Misc.Nexport.Archway.Infrastructure.RoutingRule;
using Nop.Plugin.Misc.Nexport.Archway.Migrations;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using ILogger = Nop.Services.Logging.ILogger;
using Nop.Plugin.Misc.Nexport.Archway.Factories;
using Nop.Plugin.Misc.Nexport.Archway.Filters;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure
{
    public class PluginStartup : INopStartup
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
                options.Filters.Add<ArchwayDashboardNotificationActionFilter>();
            });

            services.AddScoped<IArchwayStudentEmployeeRegistrationFieldModelFactory, ArchwayStudentEmployeeRegistrationFieldModelFactory>();
            services.AddScoped<IArchwayStudentEmployeeRegistrationFieldService, ArchwayStudentEmployeeRegistrationFieldService>();
            services.AddScoped<ArchwayPluginService>();
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
        private async Task ApplyMigration(IApplicationBuilder application)
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
                await logger.ErrorAsync($"Error occurred during database migration process: {ex.Message}", ex);
            }
        }

        public void Configure(IApplicationBuilder application)
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            if (dataSettings == null ||
                dataSettings.DataProvider == DataProviderType.Unknown ||
                string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
                return;

            var migrationTask = Task.Run(() => ApplyMigration(application));
            migrationTask.Wait();

            using var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            if (serviceScope != null)
            {
                var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();

                var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var versionSettingValue = settingService.GetSettingByKeyAsync<string>(PluginDefaults.ASSEMBLY_VERSION_KEY).Result;
                Version installedAssemblyVersion = null;

                if (!string.IsNullOrEmpty(versionSettingValue))
                {
                    installedAssemblyVersion =
                        Version.Parse(versionSettingValue);
                }

                if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                {
                    settingService.SetSettingAsync(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion?.ToString());
                   
                    var pluginService =
                        serviceScope.ServiceProvider.GetRequiredService<ArchwayPluginService>();

                    Task.Run(() => pluginService.AddOrUpdateResourcesAsync());
                }
                

                var customEnrollmentRouteControl =
                    (settingService.GetSettingByKeyAsync<bool>(PluginDefaults.CustomEnrollmentRouteControlSettingKey)).Result;
                if (customEnrollmentRouteControl)
                {
                    var customEnrollmentRoute = (settingService.GetSettingByKeyAsync<string>(PluginDefaults.CustomEnrollmentRouteSettingKey)).Result;
                    if (!string.IsNullOrWhiteSpace(customEnrollmentRoute))
                    {
                        var customShoppingCartRoutingRule = new CustomShoppingCartRoutingRule(customEnrollmentRoute);
                        var options = new RewriteOptions().Add(customShoppingCartRoutingRule);
                        application.UseRewriter(options);
                    }
                }
            }
        }

        public int Order => 11;
    }
}