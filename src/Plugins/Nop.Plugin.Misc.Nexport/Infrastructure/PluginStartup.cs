﻿using System;
using System.Reflection;
using FluentMigrator.Runner;
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
            var dataSettings = DataSettingsManager.LoadSettings();

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
            });

            services.AddFluentMigratorCore().ConfigureRunner(builder =>
            {
                builder.AddSqlServer()
                    .WithGlobalConnectionString(dataSettings.DataConnectionString)
                    .WithVersionTable(new NexportPluginMigrationVersionTable())
                    .ScanIn(typeof(M001_CreatePluginSchemas).Assembly)
                    .For.Migrations();
            }).AddLogging(lb => lb.AddEventSourceLogger());
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
                var versionSettingValue = settingService.GetSettingByKey<string>(NexportDefaults.AssemblyVersionKey);
                Version installedAssemblyVersion = null;

                if (!string.IsNullOrEmpty(versionSettingValue))
                {
                    installedAssemblyVersion =
                        Version.Parse(versionSettingValue);
                }

                if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
                {
                    settingService.SetSetting(NexportDefaults.AssemblyVersionKey, currentAssemblyVersion.ToString());

                    var nexportPluginService =
                        serviceScope.ServiceProvider.GetRequiredService<NexportPluginService>();

                    nexportPluginService.InstallScheduledTask();
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
                using (var serviceScope =
                    application.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    try
                    {
                        runner.MigrateUp();
                    }
                    catch (Exception)
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
