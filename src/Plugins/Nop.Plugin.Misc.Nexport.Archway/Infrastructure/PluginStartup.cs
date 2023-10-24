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
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Plugin.Misc.Nexport.Archway.Infrastructure.RoutingRule;
using Nop.Plugin.Misc.Nexport.Archway.Migrations;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using ILogger = Nop.Services.Logging.ILogger;
using Nop.Plugin.Misc.Nexport.Archway.Factories;

namespace Nop.Plugin.Misc.Nexport.Archway.Infrastructure;

public class PluginStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));
        services.AddScoped<IStudentEmployeeRegistrationFieldModelFactory, StudentEmployeeRegistrationFieldModelFactory>();
        services.AddScoped<IStoreEmployeePositionService, StoreEmployeePositionService>();
        services.AddScoped<IStoreEmployeeRegistrationFieldsService, StoreEmployeeRegistrationFieldsService>();
        services.AddScoped<IStudentRegistrationFieldKeyMappingService, StudentRegistrationFieldKeyMappingService>();
        services.AddScoped<IStoreRecordInfoService, StoreRecordInfoService>();
        services.AddScoped<IUploadedStoreDataFileService, UploadedStoreDataFileService>();
        services.AddScoped<PluginService>();
    }

    public static IServiceProvider CreateFluentMigratorRunnerService()
    {
        var dataSettings = DataSettingsManager.LoadSettings();
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(builder =>
            {
                builder.AddSqlServer()
                    .WithGlobalConnectionString(dataSettings.ConnectionString)
                    .WithVersionTable(new ArchwayPluginMigrationVersionTable())
                    .ScanIn(typeof(M001_CreatePluginSchemas).Assembly)
                    .For
                    .Migrations()
                    .For
                    .VersionTableMetaData();
            })
            .AddLogging(lb => lb.AddEventSourceLogger())
            .Configure<RunnerOptions>(opt => opt.Tags = new string[] { PluginDefaults.PLUGIN_MIGRATION_TAG })
            .BuildServiceProvider(false);
    }

    private static async Task ApplyMigrations()
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
            // Ignore missing migrations
            catch (MissingMigrationsException) { }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Error occurred during database migration process: {ex.Message}", ex);
        }
    }

    public void Configure(IApplicationBuilder application)
    {
        var dataSettings = DataSettingsManager.LoadSettings();
        if (dataSettings?.DataProvider is DataProviderType.Unknown or null
            || string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
        {
            return;
        }
        var migrationTask = Task.Run(ApplyMigrations);
        migrationTask.Wait();
        if (application.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope() is not { } serviceScope)
        {
            return;
        }
        var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();
        var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (!Version.TryParse(settingService.GetSettingByKeyAsync<string>(Defaults.ASSEMBLY_VERSION_KEY).Result, out var installedAssemblyVersion)
            || currentAssemblyVersion > installedAssemblyVersion)
        {
            settingService.SetSettingAsync(PluginDefaults.ASSEMBLY_VERSION_KEY, currentAssemblyVersion?.ToString());
            var pluginService = serviceScope.ServiceProvider.GetRequiredService<PluginService>();
            Task.Run(pluginService.AddOrUpdateResourcesAsync);
        }
        if (settingService.GetSettingByKeyAsync<bool>(PluginDefaults.CUSTOM_ENROLLMENT_ROUTE_CONTROL_SETTING_KEY).Result
            && settingService.GetSettingByKeyAsync<string>(PluginDefaults.CUSTOM_ENROLLMENT_ROUTE_SETTING_KEY).Result is { } rule)
        {
            var customShoppingCartRoutingRule = new CustomShoppingCartRoutingRule(rule);
            var options = new RewriteOptions().Add(customShoppingCartRoutingRule);
            application.UseRewriter(options);
        }
    }

    public int Order => 11;
}
