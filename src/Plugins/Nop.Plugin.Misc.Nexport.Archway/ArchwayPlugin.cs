using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Plugin.Misc.Nexport.Archway.Infrastructure;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Archway
{
    [CustomRegistrationFieldRender]
    public class ArchwayPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IRegistrationFieldCustomRender
    {
        private readonly ArchwayPluginService _archwayPluginService;
        private readonly IArchwayStudentEmployeeRegistrationFieldService _archwayStudentEmployeeRegistrationFieldService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        public ArchwayPlugin(
            ArchwayPluginService archwayPluginService,
            IArchwayStudentEmployeeRegistrationFieldService archwayStudentEmployeeRegistrationFieldService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            WidgetSettings widgetSetting,
            ISettingService settingService,
            IWorkContext workContext,
            IWebHelper webHelper, ILogger logger)
        {
            _archwayPluginService = archwayPluginService;
            _archwayStudentEmployeeRegistrationFieldService = archwayStudentEmployeeRegistrationFieldService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _widgetSettings = widgetSetting;
            _settingService = settingService;
            _workContext = workContext;
            _webHelper = webHelper;
            _logger = logger;
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

            await _archwayPluginService.AddOrUpdateResourcesAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _archwayPluginService.DeleteResourcesAsync();

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
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "ArchwayEmployeeRegistrationField",
                new { fieldId }, _webHelper.GetCurrentRequestProtocol());
        }

        public Task<string> GetCustomRenderUrl(int fieldId, bool renderAdminView)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return Task.FromResult(urlHelper.Action("CustomRender", "ArchwayEmployeeRegistrationField",
                new { fieldId }, _webHelper.GetCurrentRequestProtocol()));
        }

        public string GetCustomFieldPrefix()
        {
            return PluginDefaults.HtmlFieldPrefix;
        }

        public async Task<Dictionary<string, string>> ParseCustomRegistrationFields(int fieldId, IFormCollection form)
        {
            return await _archwayStudentEmployeeRegistrationFieldService
                .ParseArchwayStoreEmployeeRegistrationFields(fieldId, form);
        }

        public async Task SaveCustomRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields)
        {
            await _archwayStudentEmployeeRegistrationFieldService
                .SaveArchwayStoreEmployeeRegistrationFields(customer, fieldId, fields);
        }

        public async Task<Dictionary<string, string>> ProcessCustomRegistrationFields(int customerId, int fieldId)
        {
            return await _archwayStudentEmployeeRegistrationFieldService
                .ProcessArchwayStoreEmployeeRegistrationFields(customerId, fieldId);
        }

        public async Task<Dictionary<string, string>> GetCustomFieldNamesAndValues(int customerId, int fieldId)
        {
            return await _archwayStudentEmployeeRegistrationFieldService.GetCustomFieldNamesAndValues(customerId, fieldId);
        }

        public string GetEditCustomerRegistrationFieldAnswersViewUrl(int customerId, int fieldId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("EditCustomerRegistrationFieldAnswers", "ArchwayEmployeeRegistrationField",
                new { customerId = customerId, fieldId = fieldId }, _webHelper.GetCurrentRequestProtocol());
        }

        public async Task UpdateCustomRegistrationFieldAnswers(int customerId, int fieldId, Dictionary<string, string> fields)
        {
            await _archwayStudentEmployeeRegistrationFieldService.UpdateArchwayStudentRegistrationFieldAnswersForCustomer(customerId, fieldId, fields);
        }

        public bool HideInWidgetList => true;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == NexportDefaults.NexportCustomRegistrationFieldZone)
                return "CustomRegistrationFieldDetails";

            return "";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                NexportDefaults.NexportCustomRegistrationFieldZone
            });
        }
    }
}
