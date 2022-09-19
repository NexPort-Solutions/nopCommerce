using System;
using System.Reflection;
using System.Threading.Tasks;
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
using Nop.Plugin.Sale.CancelPendingOrderRequests.Controllers;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Filters;

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

            services.AddMvc(options =>
            {
                options.Filters.Add<CancelPendingOrderRequestsDashboardNotificationActionFilter>();
            });

            services.AddScoped<CancelPendingOrderRequestsPluginService>();
            services.AddScoped<IPendingOrderCancellationRequestService, PendingOrderCancellationRequestService>();
            services.AddScoped<CancelPendingOrderRequestsController>();
            services.AddScoped<IPendingOrderCancellationRequestModelFactory, PendingOrderCancellationRequestModelFactory>();
            services.AddScoped<IPluginLocalizationService, PluginLocalizationService>();
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
                if (currentAssemblyVersion != null)
                {
                    var versionSettingValue = settingService.GetSettingByKeyAsync<string>(PluginDefaults.ASSEMBLY_VERSION_KEY).Result;
                    Version installedAssemblyVersion = null;

                    if (!string.IsNullOrEmpty(versionSettingValue))
                    {
                        installedAssemblyVersion = Version.Parse(versionSettingValue);
                    }

                    if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                    {
                        settingService.SetSettingAsync(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion.ToString());

                        var nexportPluginService =
                            serviceScope.ServiceProvider.GetRequiredService<CancelPendingOrderRequestsPluginService>();

                        Task.Run(() => nexportPluginService.AddActivityLogTypesAsync());
                        Task.Run(() => nexportPluginService.AddMessageTemplatesAsync());
                        Task.Run(() => nexportPluginService.AddOrUpdateResourcesAsync());
                    }
                }
            }
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

        public int Order => 12;
    }
}