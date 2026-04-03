using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Hangfire;
using Hangfire.RecurringJobAdmin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexportApi.Client;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;
using Nop.Plugin.Misc.Nexport.Controllers;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Filters;
using Nop.Plugin.Misc.Nexport.Infrastructure.Logging;
using Nop.Plugin.Misc.Nexport.Migrations;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Infrastructure;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

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
        if (dataSettings == null ||
            dataSettings.DataProvider == DataProviderType.Unknown ||
            string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
            return;

        services.AddHangfire(conf => conf
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(dataSettings.ConnectionString)
            .UseRecurringJobAdmin(typeof(NopStartup).Assembly, typeof(NexportPlugin).Assembly));

        // Add the processing server as IHostedService
        var workerCount = configuration.GetValue<int?>("NexportHangfire:WorkerCount") ?? 5;

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = workerCount;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(5);
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
            options.Filters.Add<NexportWholesaleActionFilter>();
            options.Filters.Add<NexportDashboardNotificationActionFilter>();
            options.Filters.Add<SignInActionFilter>();
            options.Filters.Add<ReturnRequestActionFilter>();
        });

        var apiConfiguration = new Configuration();
        services.AddSingleton(apiConfiguration);

        services.AddTransient<ISynchronousClient>(a => new ApiClient(apiConfiguration.BasePath));
        services.AddTransient<IAsynchronousClient>(a => new ApiClient(apiConfiguration.BasePath));

        services.AddScoped<ILogger, DefaultLogger>();

        services.AddScoped<IStoreService, NexportStoreService>();
        services.AddScoped<NexportCustomerRegistrationService>();
        services.AddScoped<ICustomerRegistrationService, NexportCustomerRegistrationService>();
        services.AddScoped<IOrderProcessingService, NexportOrderProcessingService>();
        services.AddScoped<NexportApiService>();
        services.AddScoped<NexportService>();
        services.AddScoped<NexportPluginService>();
        services.AddScoped<INexportNavigationContextService, NexportNavigationContextService>();
        services.AddScoped<INexportPluginModelFactory, NexportPluginModelFactory>();
        services.AddScoped<INexportSettingModelFactory, NexportSettingModelFactory>();
        services.AddScoped<INexportWholesaleService, NexportNexportWholesaleService>();
        services.AddScoped<IScheduleJobService, ScheduleJobService>();

        services.AddScoped<NexportIntegrationController>();
        services.AddScoped<NexportSettingController>();

        //added this line because the modelstate was invalid when trying to save product mapping
        //(line 818 editmapping in nexportintegrationcontroller) which was keeping the save from happening
        //happens because we have the nullable property set in the nop.plugin.misc.nexport.csproj
        //and there are null strings in the model.
        //TODO @JS - possibly we should change string to string? in the nexportproductmappingmodel
        services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
        services.AddScoped<INexportPluginLocalizationService, NexportPluginLocalizationService>();
    }

    public static IServiceProvider CreateFluentMigratorRunnerService()
    {
        var dataSettings = DataSettingsManager.LoadSettings();

        return new ServiceCollection()
            // Add common FluentMigrator services
            .AddTransient<IDataProviderManager, DataProviderManager>()
            .AddTransient(serviceProvider => serviceProvider.GetRequiredService<IDataProviderManager>().DataProvider)
            .AddFluentMigratorCore().ConfigureRunner(builder =>
            {
                builder.AddSqlServer()
                    .WithGlobalConnectionString(dataSettings.ConnectionString)
                    .WithVersionTable(new NexportPluginMigrationVersionTable())
                    .ScanIn(typeof(Nop.Plugin.Misc.Nexport.Migrations.M001_CreatePluginSchemas).Assembly)
                    .For.Migrations()
                    .For.VersionTableMetaData();
            })
            .AddLogging(lb => lb.AddEventSourceLogger())
            .Configure<RunnerOptions>(opt =>
            {
                opt.Tags = new[] { "NexportPluginMigration" };
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
            var versionSettingValue = settingService.GetSettingByKeyAsync<string>(NexportDefaults.ASSEMBLY_VERSION_KEY).Result;
            Version installedAssemblyVersion = null;

            if (!string.IsNullOrEmpty(versionSettingValue))
            {
                installedAssemblyVersion = Version.Parse(versionSettingValue);
            }

            if (installedAssemblyVersion == null || currentAssemblyVersion > installedAssemblyVersion)
            {
                settingService.SetSettingAsync(NexportDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion?.ToString());

                var nexportPluginService =
                    serviceScope.ServiceProvider.GetRequiredService<NexportPluginService>();

                Task.Run(() => nexportPluginService.InstallScheduledTaskAsync());
                Task.Run(() => nexportPluginService.AddActivityLogTypesAsync());
                Task.Run(() => nexportPluginService.AddMessageTemplatesAsync());
                Task.Run(() => nexportPluginService.AddOrUpdateResourcesAsync());
                Task.Run(() => nexportPluginService.InstallPermissionProviderAsync());
                Task.Run(() => InitScheduleJobs(application));
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

    private async Task InitScheduleJobs(IApplicationBuilder application)
    {
        var logger = EngineContext.Current.Resolve<ILogger>();

        try
        {
            using var serviceScope = application.ApplicationServices.CreateScope();
            var scheduleJobService = serviceScope.ServiceProvider.GetRequiredService<IScheduleJobService>();
            await scheduleJobService.InitializeScheduleJobs();
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Error occurred during recurring job scheduling initialization: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 2000;
}
