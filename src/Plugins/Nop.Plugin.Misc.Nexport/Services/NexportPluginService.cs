using Nop.Core.Domain.Tasks;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportPluginService
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;

        public NexportPluginService(
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService)
        {
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
        }

        public void InstallScheduledTask()
        {
            if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportOrderProcessingTask) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = NexportDefaults.NexportOrderProcessingTaskInterval,
                    Name = NexportDefaults.NexportOrderProcessingTaskName,
                    Type = NexportDefaults.NexportOrderProcessingTask
                });
            }
        }

        public void UninstallScheduledTask()
        {
            var task = _scheduleTaskService.GetTaskByType(NexportDefaults.NexportOrderProcessingTask);
            if (task != null)
                _scheduleTaskService.DeleteTask(task);
        }

        public void AddOrUpdateResources()
        {
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Url", "Server url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Url.Hint", "The Nexport server url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Username", "Username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Username.Hint", "Nexport login username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Password", "Password");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Password.Hint", "Nexport login credential");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken", "Automatically renew token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRenewToken.Hint", "Renew access token automatically before expiring date. Does not apply if \"Token never expired\" option is selected.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired", "Token never expired");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.TokenNeverExpired.Hint", "The API access token will never expired");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate", "Token expiration date on");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CustomTokenExpirationDate.Hint", "Date that the API access token will be expired. " +
                 "If not specify, the token will be default to be expired after 30 days unless \"Automatically renew token\" or \"Token never expired\" option is selected.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Token", "Access token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate", "Token expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate.Hint", "The API access token expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.FindRootOrganization", "Find Nexport organization");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId", "Root organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId.Hint", "The organization that products will be synchronized with");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId", "Merchant account Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId.Hint", "The merchant account that will be used to process order redemptions");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId", "Nexport subscription organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint", "The Nexport subscription organization that will be used for the store");
        }

        public void DeleteResources()
        {
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Url");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Url.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Username");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Username.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Password");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Password.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Token");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcExpirationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.FindRootOrganization");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.RootOrganizationId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.MerchantAccountId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportSubscriptionOrgId.Hint");
        }
    }
}
