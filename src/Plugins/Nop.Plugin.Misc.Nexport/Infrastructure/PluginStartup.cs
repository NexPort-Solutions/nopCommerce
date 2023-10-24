using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexportApi.Client;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Filters;
using Nop.Plugin.Misc.Nexport.Infrastructure.Logging;
using Nop.Plugin.Misc.Nexport.Migrations;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Configuration;
using Nop.Services.Messages;
//using Nop.Services.Customers;
//using Nop.Services.Orders;
using ILogger = Nop.Services.Logging.ILogger;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Nop.Web.Framework.Infrastructure.Extensions;
using Microsoft.Extensions.Options;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

public class PluginStartup : INopStartup
{
    /// <inheritdoc />
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of nexportService descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new Configuration();
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));
        services.AddMvc(options =>
        {
            options.Filters.Add<ProductEditActionFilter>();
            options.Filters.Add<CheckoutActionFilter>();
            options.Filters.Add<ProductDetailsActionFilter>();
            options.Filters.Add<ShoppingCartActionFilter>();
            options.Filters.Add<OrderDetailsActionFilter>();
        });
        services.AddSingleton(apiConfiguration);
        services.AddTransient<ISynchronousClient>(_ => new ApiClient(apiConfiguration.BasePath));
        services.AddTransient<IAsynchronousClient>(_ => new ApiClient(apiConfiguration.BasePath));
        services.AddScoped<ILogger, DefaultLogger>();
        services.AddScoped<IPluginModelFactory, PluginModelFactory>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IMessageTokenProvider, CustomerMessageTokenProvider>();
        services.AddScoped<ICustomerPurchasingService, CustomerPurchasingService>();
        services.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomFieldAnswersService, CustomFieldAnswersService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<HelperService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<NexportApiService>();
        services.AddScoped<INexportService, NexportService>();
        services.AddScoped<IOrderProcessingQueueItemService, OrderProcessingQueueItemService>();
        services.AddScoped<Nop.Services.Orders.IOrderProcessingService, OrderProcessingService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<PluginService>();
        services.AddScoped<IProductGroupMembershipService, ProductGroupMembershipService>();
        services.AddScoped<IProductMappingService, ProductMappingService>();
        services.AddScoped<IRegistrationFieldService, RegistrationFieldService>();
        services.AddScoped<IRegistrationFieldModelFactory, RegistrationFieldModelFactory>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ISupplementalInfoService, SupplementalInfoService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ITrainingPlanService, TrainingPlanService>();
        services.AddScoped<IUserMappingService, UserMappingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWholesaleService, WholesaleService>();
        services.AddScoped<IWorkflowMessagingService, WorkflowMessagingService>();
        // TODO Remove when ready to refactor models with nullability.
        services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Add("/Plugins/Misc.Nexport/Views/Shared/{0}.cshtml"));
    }

    public static IServiceProvider CreateFluentMigratorRunnerService()
    {
        var dataSettings = DataSettingsManager.LoadSettings();
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder.AddSqlServer()
                .WithGlobalConnectionString(dataSettings.ConnectionString)
                .WithVersionTable(new PluginMigrationVersionTable())
                .ScanIn(typeof(M001_CreatePluginSchemas).Assembly)
                .For.Migrations()
                .For.VersionTableMetaData())
            .AddLogging(loggingBuilder => loggingBuilder.AddEventSourceLogger())
            .Configure<RunnerOptions>(option => option.Tags = new[] { PLUGIN_MIGRATION_TAG })
            .BuildServiceProvider(false);
    }

    /// <inheritdoc />
    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
        application.UseDeveloperExceptionPage(new() { SourceCodeLineCount = 10 });
        application.UseNopExceptionHandler();
        if (DataSettingsManager.LoadSettings() is not { } dataSettings
            || dataSettings.DataProvider is DataProviderType.Unknown
            || string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
        {
            return;
        }
        var migrationTask = Task.Run(ApplyMigrations);
        migrationTask.Wait();
        using var serviceScope = application.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope is null)
        {
            return;
        }
        var settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();
        var pluginService = serviceScope.ServiceProvider.GetRequiredService<PluginService>();
        Task.Run(() => ConfigureVersion(settingService, pluginService));
    }

    private static async Task ConfigureVersion(ISettingService settingService, PluginService pluginService)
    {
        var currentAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var versionSettingValue = await settingService.GetSettingByKeyAsync<string>(ASSEMBLY_VERSION_KEY);
        if (!Version.TryParse(versionSettingValue, out var installedAssemblyVersion)
            || currentAssemblyVersion > installedAssemblyVersion)
        {
            UpgradeVersion(pluginService, settingService, currentAssemblyVersion);
        }
    }

    private static void UpgradeVersion(PluginService pluginService, ISettingService settingService, Version? currentAssemblyVersion)
    {
        settingService.SetSettingAsync(ASSEMBLY_VERSION_KEY, currentAssemblyVersion?.ToString());
        Task.Run(pluginService.InstallScheduledTaskAsync);
        Task.Run(pluginService.AddActivityLogTypesAsync);
        Task.Run(pluginService.AddMessageTemplatesAsync);
        Task.Run(pluginService.AddOrUpdateResourcesAsync);
        Task.Run(pluginService.InstallPermissionProviderAsync);
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
            catch (MissingMigrationsException)
            {
                // ignored
            }
        }
        catch (Exception exception)
        {
            await logger.ErrorAsync($"Error occurred during database migration process: {exception.Message}", exception);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 2000;
}
