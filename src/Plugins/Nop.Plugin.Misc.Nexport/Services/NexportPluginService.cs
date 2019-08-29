using System;
using Nop.Core.Domain.Tasks;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportPluginService
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public NexportPluginService(
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService,
            ILogger logger)
        {
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void InstallScheduledTask()
        {
            try
            {
                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportOrderProcessingTask) == null)
                {
                    var orderProcessingTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportOrderProcessingTaskInterval,
                        Name = NexportDefaults.NexportOrderProcessingTaskName,
                        Type = NexportDefaults.NexportOrderProcessingTask
                    };
                    _scheduleTaskService.InsertTask(orderProcessingTask);

                    var task = new Task(orderProcessingTask);
                    task.Execute(true, false);
                }

                if (_scheduleTaskService.GetTaskByType(NexportDefaults.NexportSynchronizationTask) == null)
                {
                    var synchronizationTask = new ScheduleTask
                    {
                        Enabled = true,
                        Seconds = NexportDefaults.NexportSynchronizationTaskInterval,
                        Name = NexportDefaults.NexportSynchronizationTaskName,
                        Type = NexportDefaults.NexportSynchronizationTask
                    };
                    _scheduleTaskService.InsertTask(synchronizationTask);

                    var task = new Task(synchronizationTask);
                    task.Execute(true, false);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Cannot install and run new task(s)", e);
            }
        }

        public void UninstallScheduledTask()
        {
            var tasks = _scheduleTaskService.GetAllTasks();

            foreach (var task in tasks)
            {
                if (task.Type.Equals(NexportDefaults.NexportOrderProcessingTask) ||
                    task.Type.Equals(NexportDefaults.NexportSynchronizationTask))
                {
                    _scheduleTaskService.DeleteTask(task);
                }
            }
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

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName", "Product name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName.Hint", "The name of the product in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DisplayName", "Display name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.DisplayName.Hint", "The display name in Nop for the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId", "Catalog syllabus link Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint", "The Id of the catalog syllabus linking");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogId", "Catalog Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CatalogId.Hint", "The Id of the Nexport catalog");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId","Syllabus Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId.Hint","The Id of the Nexport syllabus");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId", "Subscription organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId.Hint", "The Id of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName", "Subscription organization name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName.Hint", "The name of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName", "Subscription organization short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint", "The short name of the subscription organization in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Type", "Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Type.Hint", "The type of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel", "Publishing model");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel.Hint", "The publishing model in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PricingModel", "Pricing model");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.PricingModel.Hint", "The pricing model in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate", "Modified date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate.Hint", "The last modified date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate", "Available date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate.Hint", "The available date of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.EndDate", "End date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.EndDate.Hint", "The ending date of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CreditHours", "Credit hours");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.CreditHours.Hint", "The credit hours of the Nexport product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus", "Section CEUs");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus.Hint", "The CEUs for the Nexport section");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized", "Synchronized?");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized.Hint", "Indication of synchronization from Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate", "Last synchronization date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate.Hint", "The date that the last synchronization occurred");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem", "Auto redeeming purchase");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem.Hint", "Allows customers to redeem the product right after purchasing");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption", "Access expiration option");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint",
                "Specify the access expiration in Nexport. If specified, access expiration date will take priority compared to access expiration time limit.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate", "Access expiration date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint",
                "The access expiration date (in Coordinated Universal Time - UTC) in Nexport. The product access expiration date will be based on this date instead.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit", "Access expiration time limit");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit.Hint",
                "The access time limit in Nexport. The product access expiration date will be based on the date of the product redemption plus the access time limit.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupId", "Group Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupId.Hint", "The Id of the group in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupName", "Group name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupName.Hint", "The name of the group in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName", "Group short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName.Hint", "The short name of the group in Nexport");

            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.UserId", "Nexport user Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.UserId.Hint", "The user Id in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId", "Nexport owner organization Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId.Hint", "The Id of the owner organization for the user");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName", "Nexport owner organization short name");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName.Hint", "The short name of the owner organization for the user");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.Email", "Nexport email");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Fields.Nexport.Email.Hint", "The internal email of the user in Nexport");
            _localizationService.AddOrUpdatePluginLocaleResource("Account.Login.Fields.EmailOrUsername", "Email/Username");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.UserInfo", "Nexport user info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyTrainings", "My trainings");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Order.ViewRedemption", "Launch this training");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Nexport.Order.Redeem", "Redeem");
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

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.NexportProductName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DisplayName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.DisplayName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogSyllabusLinkId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CatalogId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SyllabusId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SubscriptionOrgShortName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Type");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Type.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PublishingModel.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PricingModel");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.PricingModel.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ModifiedDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AvailableDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.EndDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.EndDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CreditHours");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.CreditHours.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.SectionCeus.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.IsSynchronized.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.LastSynchronizationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AutoRedeem.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.ProductAccessExpirationOption.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.UtcAccessExpirationDate.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.AccessTimeLimit.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.GroupShortName.Hint");

            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.UserId");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.UserId.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgId.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.OwnerOrgShortName.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.Email");
            _localizationService.DeletePluginLocaleResource("Account.Fields.Nexport.Email.Hint");
            _localizationService.DeletePluginLocaleResource("Account.Login.Fields.EmailOrUsername");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.UserInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Navigation.MyTrainings");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Order.ViewRedemption");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Nexport.Order.Redeem");
        }
    }
}
