using System;
using System.Collections.Generic;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
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

        public override void Install()
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
                _logger.Error($"Error occurred during database migration process: {ex.Message}", ex);
            }

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _archwayPluginService.AddOrUpdateResources();

            base.Install();
        }

        public override void Uninstall()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDefaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _archwayPluginService.DeleteResources();

            try
            {
                var runner = EngineContext.Current.Resolve<IMigrationRunner>();
                runner.MigrateDown(0);

                ((MigrationRunner)runner).VersionLoader.RemoveVersionTable();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while removing plugin {PluginDefaults.SystemName} version table : {ex.Message}", ex);
            }

            base.Uninstall();
        }

        public string GetRenderOptionUrl(int fieldId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "ArchwayEmployeeRegistrationField",
                new { fieldId = fieldId }, _webHelper.CurrentRequestProtocol);
        }

        public string GetCustomRenderUrl(int fieldId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("CustomRender", "ArchwayEmployeeRegistrationField",
                new { fieldId = fieldId }, _webHelper.CurrentRequestProtocol);
        }

        public string GetCustomFieldPrefix()
        {
            return PluginDefaults.HtmlFieldPrefix;
        }

        public Dictionary<string, string> ParseCustomRegistrationFields(int fieldId, IFormCollection form)
        {
            return _archwayStudentEmployeeRegistrationFieldService
                .ParseArchwayStoreEmployeeRegistrationFields(fieldId, form);
        }

        public void SaveCustomRegistrationFields(int fieldId, Dictionary<string, string> fields)
        {
            _archwayStudentEmployeeRegistrationFieldService
                .SaveArchwayStoreEmployeeRegistrationFields(_workContext.CurrentCustomer, fieldId, fields);
        }

        public Dictionary<string, string> ProcessCustomRegistrationFields(int customerId, int fieldId)
        {
            return _archwayStudentEmployeeRegistrationFieldService
                .ProcessArchwayStoreEmployeeRegistrationFields(customerId, fieldId);
        }

        public bool HideInWidgetList => true;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == NexportDefaults.NexportCustomRegistrationFieldZone)
                return "CustomRegistrationFieldDetails";

            return "";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>()
            {
                NexportDefaults.NexportCustomRegistrationFieldZone
            };
        }
    }
}
