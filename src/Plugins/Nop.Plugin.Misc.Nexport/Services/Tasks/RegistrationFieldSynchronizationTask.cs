using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class RegistrationFieldSynchronizationTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly INexportService _nexportService;
    private readonly IUserMappingService _userMapping;
    private readonly IRepository<SynchronizationQueueItem> _registrationFieldSynchronizationQueues;
    private readonly IRegistrationFieldService _registrationField;
    private readonly ICustomFieldAnswersService _customField;
    private readonly ISubscriptionService _subscription;
    private readonly int _batchSize = 100;

    private const int MAX_ATTEMPT_COUNT = 5;

    public RegistrationFieldSynchronizationTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        IRepository<SynchronizationQueueItem> registrationFieldSynchronizationQueues,
        INexportService nexportService,
        IUserMappingService userMapping,
        IRegistrationFieldService registrationField,
        ICustomFieldAnswersService customField,
        ISubscriptionService subscription)
    {
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _registrationFieldSynchronizationQueues = registrationFieldSynchronizationQueues;
        _nexportService = nexportService;
        _userMapping = userMapping;
        _registrationField = registrationField;
        _customField = customField;
        _subscription = subscription;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(SystemNames.SYSTEM_NAME))
        {
            return;
        }
        try
        {
            var syncItemIds = await _registrationFieldSynchronizationQueues.Table
                .OrderBy(item => item.UtcDateLastAttempt)
                .ThenBy(item => item.UtcDateCreated)
                .Select(item => item.Id)
                .Take(_batchSize)
                .ToListAsync();
            await SynchronizeRegistrationFieldsAsync(syncItemIds);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot synchronize registration fields with NexPort", exception);
        }
    }

    public async Task SynchronizeRegistrationFieldsAsync(IList<int> queueItemIds)
    {
        foreach (var queueItemId in queueItemIds)
        {
            try
            {
                if (await _registrationFieldSynchronizationQueues.GetByIdAsync(queueItemId) is not { } syncItem
                    || await _userMapping.FindByCustomerId(syncItem.CustomerId) is not { } userMapping)
                {
                    continue;
                }
                await _logger.DebugAsync($"Begin registration fields synchronization for customer {syncItem.CustomerId}");
                if (syncItem.Attempt > MAX_ATTEMPT_COUNT)
                {
                    await _registrationFieldSynchronizationQueues.DeleteAsync(syncItem);
                    continue;
                }
                try
                {
                    // Synchronize customer custom profile fields with NexPort
                    await SynchronizeRegistrationFields(userMapping);
                    await _registrationFieldSynchronizationQueues.DeleteAsync(syncItem);
                }
                catch (Exception)
                {
                    syncItem.Attempt++;
                    if (syncItem.Attempt <= MAX_ATTEMPT_COUNT)
                    {
                        await _registrationField.UpdateSynchronizationQueueItem(syncItem);
                    }
                    else
                    {
                        await _registrationFieldSynchronizationQueues.DeleteAsync(syncItem);
                    }
                }
                await _logger.DebugAsync("Synchronize registration fields in NexPort completed.");
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync($"Cannot process the RegistrationFieldSynchronizationQueue item with GroupGuid {queueItemId}", exception);
            }
        }
    }

    private async Task SynchronizeRegistrationFields(UserMapping userMapping)
    {
        var registrationFields = await _registrationField.GetAnswers(userMapping.NopUserId);
        var profileFields = await _customField.ConvertFieldAnswersToSubmissionProfileFields(registrationFields.Where(answer => !answer.IsCustomField));
        var customFields = await _customField.ConvertCustomFieldAnswersToSubmissionProfileFieldsAsync(registrationFields.Where(answer => answer.IsCustomField));
        var finalFields = profileFields.Concat(customFields).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        foreach (var subscription in await _subscription.FindAllSubscriptions(userMapping.UserId))
        {
            var result = await _nexportService.SetCustomProfileFieldValues(subscription.SubscriptionId, finalFields);
            if (!string.IsNullOrEmpty(result?.Message))
            {
                await _logger.ErrorAsync($"Error occurred when setting custom profile fields for customer {userMapping.NopUserId}: {result.Message}");
            }
            var message = $"Successfully synchronize custom profile fields for customer {userMapping.NopUserId} with the subscriber GroupGuid {subscription.SubscriptionId}";
            await _logger.InformationAsync(message);
        }
        await _logger.InformationAsync($"Successfully synchronize all custom profile fields for customer {userMapping.NopUserId}");
    }
}
