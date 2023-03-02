using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexportApi.Model;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

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

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                var syncItemIds = await _nexportRegistrationFieldSynchronizationQueueRepository.Table
                    .OrderBy(item => item.UtcDateLastAttempt)
                    .ThenBy(item => item.UtcDateCreated)
                    .Select(item => item.Id)
                    .Take(_batchSize)
                    .ToListAsync();

                await SynchronizeRegistrationFieldsAsync(syncItemIds);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot synchronize registration fields with Nexport", ex);
            }
        }

        public async Task SynchronizeRegistrationFieldsAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var syncItem = await _nexportRegistrationFieldSynchronizationQueueRepository.GetByIdAsync(queueItemId);

                        if (syncItem == null)
                            return;

                        await _logger.DebugAsync($"Begin registration fields synchronization for customer {syncItem.CustomerId}");

                        var userMapping = await _nexportService.FindUserMappingByCustomerId(syncItem.CustomerId);
                        if (userMapping != null)
                        {
                            if (syncItem.Attempt > MAX_ATTEMPT_COUNT)
                            {
                                await _nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                            }
                            else
                            {
                                try
                                {
                                    // Synchronize customer custom profile fields with Nexport
                                    await SynchronizeRegistrationFields(syncItem, userMapping);

                                    await _nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                                }
                                catch (Exception)
                                {
                                    syncItem.Attempt++;

                                    if (syncItem.Attempt <= MAX_ATTEMPT_COUNT)
                                    {
                                        await _nexportService.UpdateNexportRegistrationFieldSynchronizationQueueItem(syncItem);
                                    }
                                    else
                                    {
                                        await _nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                                    }
                                }

                                await _logger.DebugAsync("Synchronize registration fields in Nexport completed.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process the NexportRegistrationFieldSynchronizationQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot synchronize registration field with Nexport", ex);
            }
        }

        private async Task SynchronizeRegistrationFields(NexportRegistrationFieldSynchronizationQueueItem syncItem, NexportUserMapping userMapping)
        {
            if (syncItem == null)
                throw new ArgumentNullException(nameof(syncItem));

            if (userMapping == null)
                throw new ArgumentNullException(nameof(userMapping));

            var registrationFields = await _nexportService.GetNexportRegistrationFieldAnswers(userMapping.NopUserId);

            if (registrationFields.Count > 0)
            {
                List<SubscriptionResponse> nexportSubscriptions;

                try
                {
                    nexportSubscriptions = (await _nexportService.FindAllSubscriptionsAsync(userMapping.NexportUserId)).ToList();
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Cannot retrieve subscriptions for customer {userMapping.NopUserId} in Nexport", ex);
                    throw;
                }

                if (nexportSubscriptions.Count > 0)
                {
                    var profileFields =
                        await _nexportService.ConvertFieldAnswersToSubmissionProfileFields(
                            registrationFields.Where(x => !x.IsCustomField).ToList());

                    var customFields =
                        await _nexportService.ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(
                            registrationFields.Where(x => x.IsCustomField).ToList());

                    var finalFields = profileFields.Concat(customFields)
                        .ToDictionary(x => x.Key,
                            x => x.Value);

                    foreach (var subscription in nexportSubscriptions)
                    {
                        try
                        {
                            var result = await _nexportService.SetCustomProfileFieldValuesAsync(subscription.SubscriptionId, finalFields);

                            if (result != null && !string.IsNullOrEmpty(result.Message))
                            {
                                await _logger.ErrorAsync($"Error occurred when setting custom profile fields for customer {userMapping.NopUserId}: {result.Message}");
                            }

                            await _logger.InformationAsync($"Successfully synchronize custom profile fields for customer {userMapping.NopUserId} with the subscriber Id {subscription.SubscriptionId}");
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync($"Cannot set custom profile fields for customer {userMapping.NopUserId} in Nexport", ex);
                            throw;
                        }
                    }

                    await _logger.InformationAsync($"Successfully synchronize all custom profile fields for customer {userMapping.NopUserId}");
                }
            }
        }
    }
}
