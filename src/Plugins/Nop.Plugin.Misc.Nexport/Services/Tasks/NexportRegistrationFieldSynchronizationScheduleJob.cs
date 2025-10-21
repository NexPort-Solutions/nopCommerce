using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Logging;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

/// <summary>
/// Represents a task for synchronizing registration fields with Nexport
/// </summary>
public class NexportRegistrationFieldSynchronizationScheduleJob(
    IWidgetPluginManager widgetPluginManager,
    ILogger logger,
    IRepository<NexportRegistrationFieldSynchronizationQueueItem>
        nexportRegistrationFieldSynchronizationQueueRepository,
    NexportService nexportService)
    : INexportScheduleJob
{
    private int _batchSize = 100;
    private const int MAX_ATTEMPT_COUNT = 5;

    public string JobName { get; set; } = "NexportRegistrationFieldSynchronization";

    public long Interval { get; set; } = 300; // Default to 5 minutes

    public async Task ExecuteAsync()
    {
        if (!await widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
        {
            await logger.WarningAsync("RegistrationFieldSynchronization job cannot be executed due to Nexport plugin is not currently active!");
            return;
        }

        try
        {
            var syncItemIds = await nexportRegistrationFieldSynchronizationQueueRepository.Table
                .OrderBy(item => item.UtcDateLastAttempt)
                .ThenBy(item => item.UtcDateCreated)
                .Select(item => item.Id)
                .Take(_batchSize)
                .ToListAsync();

            await SynchronizeRegistrationFieldsAsync(syncItemIds);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot synchronize registration fields with Nexport", ex);
        }
    }

    private async Task SynchronizeRegistrationFieldsAsync(IList<int> queueItemIds)
    {
        try
        {
            foreach (var queueItemId in queueItemIds)
            {
                try
                {
                    var syncItem = await nexportRegistrationFieldSynchronizationQueueRepository.GetByIdAsync(queueItemId);

                    if (syncItem == null)
                        return;

                    await logger.DebugAsync($"Begin registration fields synchronization for customer {syncItem.CustomerId}");

                    var userMapping = await nexportService.FindUserMappingByCustomerId(syncItem.CustomerId);
                    if (userMapping != null)
                    {
                        if (syncItem.Attempt > MAX_ATTEMPT_COUNT)
                        {
                            await nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                        }
                        else
                        {
                            try
                            {
                                // Synchronize customer custom profile fields with Nexport
                                await SynchronizeRegistrationFields(syncItem, userMapping);

                                await nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                            }
                            catch (Exception)
                            {
                                syncItem.Attempt++;

                                if (syncItem.Attempt <= MAX_ATTEMPT_COUNT)
                                {
                                    await nexportService.UpdateNexportRegistrationFieldSynchronizationQueueItem(syncItem);
                                }
                                else
                                {
                                    await nexportRegistrationFieldSynchronizationQueueRepository.DeleteAsync(syncItem);
                                }
                            }

                            await logger.DebugAsync("Synchronize registration fields in Nexport completed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync($"Cannot process the NexportRegistrationFieldSynchronizationQueue item with Id {queueItemId}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot synchronize registration field with Nexport", ex);
        }
    }

    private async Task SynchronizeRegistrationFields(NexportRegistrationFieldSynchronizationQueueItem syncItem, NexportUserMapping userMapping)
    {
        if (syncItem == null)
            throw new ArgumentNullException(nameof(syncItem));

        if (userMapping == null)
            throw new ArgumentNullException(nameof(userMapping));

        var registrationFields = await nexportService.GetNexportRegistrationFieldAnswers(userMapping.NopUserId);

        if (registrationFields.Count > 0)
        {
            List<SubscriptionResponse> nexportSubscriptions;

            try
            {
                nexportSubscriptions = (await nexportService.FindAllSubscriptionsAsync(userMapping.NexportUserId)).ToList();
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Cannot retrieve subscriptions for customer {userMapping.NopUserId} in Nexport", ex);
                throw;
            }

            if (nexportSubscriptions.Count > 0)
            {
                var profileFields =
                    await nexportService.ConvertFieldAnswersToSubmissionProfileFields(
                        registrationFields.Where(x => !x.IsCustomField).ToList());

                var customFields =
                    await nexportService.ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(
                        registrationFields.Where(x => x.IsCustomField).ToList());

                var finalFields = profileFields.Concat(customFields)
                    .ToDictionary(x => x.Key,
                        x => x.Value);

                foreach (var subscription in nexportSubscriptions)
                {
                    try
                    {
                        var result = await nexportService.SetCustomProfileFieldValuesAsync(subscription.SubscriptionId, finalFields);

                        if (result != null && !string.IsNullOrEmpty(result.Message))
                        {
                            await logger.ErrorAsync($"Error occurred when setting custom profile fields for customer {userMapping.NopUserId}: {result.Message}");
                        }

                        await logger.InformationAsync($"Successfully synchronize custom profile fields for customer {userMapping.NopUserId} with the subscriber Id {subscription.SubscriptionId}");
                    }
                    catch (Exception ex)
                    {
                        await logger.ErrorAsync($"Cannot set custom profile fields for customer {userMapping.NopUserId} in Nexport", ex);
                        throw;
                    }
                }

                await logger.InformationAsync($"Successfully synchronize all custom profile fields for customer {userMapping.NopUserId}");
            }
        }
    }
}