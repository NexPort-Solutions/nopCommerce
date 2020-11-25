using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportRegistrationFieldSynchronizationTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly NexportService _nexportService;
        private readonly IRepository<NexportRegistrationFieldSynchronizationQueueItem> _nexportRegistrationFieldSynchronizationQueueRepository;

        private int _batchSize = 100;
        private const int MAX_ATTEMPT_COUNT = 5;

        public NexportRegistrationFieldSynchronizationTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            IRepository<NexportRegistrationFieldSynchronizationQueueItem> nexportRegistrationFieldSynchronizationQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _nexportRegistrationFieldSynchronizationQueueRepository = nexportRegistrationFieldSynchronizationQueueRepository;
            _nexportService = nexportService;
        }

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                var syncItemIds = _nexportRegistrationFieldSynchronizationQueueRepository.Table
                    .OrderBy(item => item.UtcDateLastAttempt)
                    .ThenBy(item => item.UtcDateCreated)
                    .Select(item => item.Id)
                    .Take(_batchSize).ToList();

                SynchronizeRegistrationFields(syncItemIds);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize registration fields with Nexport", ex);
            }
        }

        public void SynchronizeRegistrationFields(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var syncItem = _nexportRegistrationFieldSynchronizationQueueRepository.GetById(queueItemId);

                        if (syncItem == null)
                            return;

                        _logger.Debug($"Begin registration fields synchronization for customer {syncItem.CustomerId}");

                        var userMapping = _nexportService.FindUserMappingByCustomerId(syncItem.CustomerId);
                        if (userMapping != null)
                        {
                            if (syncItem.Attempt > MAX_ATTEMPT_COUNT)
                            {
                                _nexportRegistrationFieldSynchronizationQueueRepository.Delete(syncItem);
                            }
                            else
                            {
                                try
                                {
                                    // Synchronize customer custom profile fields with Nexport
                                    SynchronizeRegistrationFields(syncItem, userMapping);

                                    _nexportRegistrationFieldSynchronizationQueueRepository.Delete(syncItem);
                                }
                                catch (Exception)
                                {
                                    syncItem.Attempt++;

                                    if (syncItem.Attempt <= MAX_ATTEMPT_COUNT)
                                    {
                                        _nexportService.UpdateNexportRegistrationFieldSynchronizationQueueItem(syncItem);
                                    }
                                    else
                                    {
                                        _nexportRegistrationFieldSynchronizationQueueRepository.Delete(syncItem);
                                    }
                                }

                                _logger.Debug("Synchronize registration fields in Nexport completed.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportRegistrationFieldSynchronizationQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize registration field with Nexport", ex);
            }
        }

        private void SynchronizeRegistrationFields(NexportRegistrationFieldSynchronizationQueueItem syncItem,
            NexportUserMapping userMapping)
        {
            if (syncItem == null)
                throw new ArgumentNullException(nameof(syncItem));

            if (userMapping == null)
                throw new ArgumentNullException(nameof(userMapping));

            var registrationFields = _nexportService.GetNexportRegistrationFieldAnswers(userMapping.NopUserId);

            if (registrationFields.Count > 0)
            {
                List<SubscriptionResponse> nexportSubscriptions;

                try
                {
                    nexportSubscriptions = _nexportService.FindAllSubscriptions(userMapping.NexportUserId).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Cannot retrieve subscriptions for customer {userMapping.NopUserId} in Nexport", ex);
                    throw;
                }

                if (nexportSubscriptions.Count > 0)
                {
                    var profileFields =
                        _nexportService.ConvertFieldAnswersToSubmissionProfileFields(
                            registrationFields.Where(x => !x.IsCustomField).ToList());

                    var customFields =
                        _nexportService.ConvertCustomFieldAnswersToSubmissionProfileFields(
                            registrationFields.Where(x => x.IsCustomField).ToList());

                    var finalFields = profileFields.Concat(customFields)
                        .ToDictionary(x => x.Key,
                            x => x.Value);

                    foreach (var subscription in nexportSubscriptions)
                    {
                        try
                        {
                            var result = _nexportService.SetCustomProfileFieldValues(subscription.SubscriptionId, finalFields);

                            if (result != null && !string.IsNullOrEmpty(result.Message))
                            {
                                _logger.Error($"Error occurred when setting custom profile fields for customer {userMapping.NopUserId}: {result.Message}");
                            }

                            _logger.Information($"Successfully synchronize custom profile fields for customer {userMapping.NopUserId} with the subscriber Id {subscription.SubscriptionId}");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Cannot set custom profile fields for customer {userMapping.NopUserId} in Nexport", ex);
                            throw;
                        }
                    }

                    _logger.Information($"Successfully synchronize all custom profile fields for customer {userMapping.NopUserId}");
                }
            }
        }
    }
}
