using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using AdminController = Nop.Plugin.Misc.Nexport.Archway.Areas.Admin.Controllers.EmployeeRegistrationFieldController;
using Controller = Nop.Plugin.Misc.Nexport.Archway.Controllers.EmployeeRegistrationFieldController;
using PluginStartup = Nop.Plugin.Misc.Nexport.Archway.Infrastructure.PluginStartup;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;

namespace Nop.Plugin.Misc.Nexport.Archway;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

[CustomRegistrationFieldRender]
public class ArchwayPlugin : Nop.Services.Plugins.BasePlugin, IMiscPlugin, IRegistrationFieldCustomRender
{
    private readonly Services.PluginService _pluginService;
    private readonly IUploadedStoreDataFileService _uploadedStoreDataFileService;
    private readonly IStudentRegistrationFieldAnswerService _studentRegistrationFieldAnswerService;
    private readonly IStoreEmployeeRegistrationFieldsService _storeEmployeeRegistrationFieldsService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    private readonly ILogger _logger;

    public ArchwayPlugin(
        Services.PluginService archwayPluginService,
        IUploadedStoreDataFileService archwayStudentEmployeeRegistrationFieldService,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        WidgetSettings widgetSetting,
        ISettingService settingService,
        IWorkContext workContext,
        IWebHelper webHelper,
        ILogger logger,
        IStoreEmployeeRegistrationFieldsService storeEmployeeRegistrationFieldsService,
        IStudentRegistrationFieldAnswerService studentRegistrationFieldAnswerService)
    {
        _pluginService = archwayPluginService;
        _uploadedStoreDataFileService = archwayStudentEmployeeRegistrationFieldService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _widgetSettings = widgetSetting;
        _settingService = settingService;
        _workContext = workContext;
        _webHelper = webHelper;
        _logger = logger;
        _storeEmployeeRegistrationFieldsService = storeEmployeeRegistrationFieldsService;
        _studentRegistrationFieldAnswerService = studentRegistrationFieldAnswerService;
    }

    public override async Task InstallAsync()
    {
        try
        {
            var migratorRunnerService = PluginStartup.CreateFluentMigratorRunnerService();
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
            await _logger.ErrorAsync($"Error occurred during database migration process: {ex.Message}", ex);
        }

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _pluginService.AddOrUpdateResourcesAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _pluginService.DeleteResourcesAsync();

        try
        {
            var migratorRunnerService = PluginStartup.CreateFluentMigratorRunnerService();
            using var serviceScope = migratorRunnerService.CreateScope();
            var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            try
            {
                runner.MigrateDown(0);
                ((MigrationRunner)runner).VersionLoader.RemoveVersionTable();
            }
            catch (MissingMigrationsException)
            {
                // ignored
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error occurred while removing plugin {PluginDefaults.SystemName} version table : {ex.Message}", ex);
        }

        await base.UninstallAsync();
    }

    public string GetRenderOptionUrl(int fieldId)
        => _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext!)
            .Action(nameof(AdminController.Configure), ControllerUtilities.GetControllerName<AdminController>(), new { fieldId }, _webHelper.GetCurrentRequestProtocol())
            ?? throw new InvalidOperationException($"Missing url for: {nameof(GetRenderOptionUrl)}({fieldId})");

    public string GetEditCustomerRegistrationFieldAnswersViewUrl(int customerId, int fieldId)
        => _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext!)
            .Action(
                nameof(AdminController.EditCustomerRegistrationFieldAnswers),
                ControllerUtilities.GetControllerName<AdminController>(),
                new { customerId = customerId, fieldId = fieldId },
                _webHelper.GetCurrentRequestProtocol())
            ?? throw new InvalidOperationException($"Missing url for: {nameof(GetEditCustomerRegistrationFieldAnswersViewUrl)}({customerId}, {fieldId})");

    public async Task<string> GetCustomRenderUrl(int fieldId, bool renderAdminView)
        => _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext!)
            .Action(nameof(Controller.CustomRender), ControllerUtilities.GetControllerName<Controller>(), new { fieldId }, _webHelper.GetCurrentRequestProtocol())
            ?? throw new InvalidOperationException($"Missing url for: {nameof(GetCustomRenderUrl)}({fieldId}, {renderAdminView})");

    public string GetCustomFieldPrefix() => PluginDefaults.HTML_FIELD_PREFIX;

    public async Task<Dictionary<string, string>> ParseCustomRegistrationFields(int fieldId, IFormCollection form)
        => _storeEmployeeRegistrationFieldsService.Parse(fieldId, form);

    public Task SaveCustomRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields)
        => _storeEmployeeRegistrationFieldsService.Save(customer, fieldId, fields);

    public Task<Dictionary<string, string>> ProcessCustomRegistrationFields(int customerId, int fieldId)
        => _storeEmployeeRegistrationFieldsService.Process(customerId, fieldId);

    public Task<Dictionary<string, string>> GetCustomFieldNamesAndValues(int customerId, int fieldId)
        => _storeEmployeeRegistrationFieldsService.GetById(customerId, fieldId);

    public async Task UpdateCustomRegistrationFieldAnswers(int customerId, int fieldId, Dictionary<string, string> fields)
        => await _studentRegistrationFieldAnswerService.BulkUpdateForCustomer(customerId, fieldId, fields);

    public static bool HideInWidgetList => true;
}
