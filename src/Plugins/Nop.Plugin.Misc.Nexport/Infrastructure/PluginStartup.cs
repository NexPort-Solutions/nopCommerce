using System;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Misc.Nexport.Data;
using Nop.Plugin.Misc.Nexport.Filters;
using Nop.Plugin.Misc.Nexport.Migrations;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        /// <inheritdoc />
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add object context
            services.AddDbContext<NexportPluginObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            // Add action filters
            services.AddMvc(options =>
            {
                options.Filters.Add<ProductEditActionFilter>();
                options.Filters.Add<CheckoutActionFilter>();
                options.Filters.Add<ProductDetailsActionFilter>();
                options.Filters.Add<ShoppingCartActionFilter>();
                options.Filters.Add<OrderDetailsActionFilter>();
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
                        .WithVersionTable(new NexportPluginMigrationVersionTable())
                        .ScanIn(typeof(Nop.Plugin.Misc.Nexport.Migrations.M001_CreatePluginSchemas).Assembly)
                        .For.Migrations()
                        .For.VersionTableMetaData();
                })
                .AddLogging(lb => lb.AddEventSourceLogger())
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Tags = new[] { "", "NexportPluginMigration" };
                })
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
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
                var versionSettingValue = settingService.GetSettingByKey<string>(NexportDefaults.ASSEMBLY_VERSION_KEY);
                Version installedAssemblyVersion = null;

                if (!string.IsNullOrEmpty(versionSettingValue))
                {
                    installedAssemblyVersion =
                        Version.Parse(versionSettingValue);
                }

                if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                {
                    settingService.SetSetting(NexportDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion.ToString());

                    var nexportPluginService =
                        serviceScope.ServiceProvider.GetRequiredService<NexportPluginService>();

                    nexportPluginService.InstallScheduledTask();
                    nexportPluginService.AddActivityLogTypes();
                    nexportPluginService.AddOrUpdateResources();
                }
            }
        }

        /// <summary>
        /// Apply migrations
        /// </summary>
        /// <param name="application"></param>
        private void ApplyMigration(IApplicationBuilder application)
        {
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
            catch (Exception)
            {
                // ignored
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 11;
    }
}
