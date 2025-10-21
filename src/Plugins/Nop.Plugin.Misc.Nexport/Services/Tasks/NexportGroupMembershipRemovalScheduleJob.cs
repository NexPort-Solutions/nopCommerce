using NexportApi.Client;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class NexportGroupMembershipRemovalScheduleJob(
    IWidgetPluginManager widgetPluginManager,
    ILogger logger,
    ICustomerService customerService,
    ICustomerActivityService customerActivityService,
    ISettingService settingService,
    IRepository<NexportGroupMembershipRemovalQueueItem> nexportGroupMembershipRemovalQueueRepository,
    NexportService nexportService)
    : INexportScheduleJob
{
    private int _batchSize;

    public string JobName { get; set; } = "NexportGroupMembershipRemoval";

    public long Interval { get; set; } = 30; // Default to 30 seconds

    public async Task ExecuteAsync()
    {
        if (!await widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
        {
            await logger.WarningAsync("GroupMembershipRemoval job cannot be executed due to Nexport plugin is not currently active!");
            return;
        }

        try
        {
            _batchSize = await settingService.GetSettingByKeyAsync(
                NexportDefaults.NexportGroupMembershipRemovalTaskBatchSizeSettingKey,
                NexportDefaults.NexportGroupMembershipRemovalTaskBatchSize);

            var answers = await nexportGroupMembershipRemovalQueueRepository.Table
                .OrderBy(q => q.UtcDateCreated)
                .Select(q => q.Id)
                .Take(_batchSize)
                .ToListAsync();

            await ProcessNexportGroupMembershipRemovalAsync(answers);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Cannot process Nexport group membership removal", ex);
        }
    }

    public async Task ProcessNexportGroupMembershipRemovalAsync(IList<int> queueItemIds)
    {
        try
        {
            foreach (var queueItemId in queueItemIds)
            {
                try
                {
                    var queueItem = await nexportGroupMembershipRemovalQueueRepository.GetByIdAsync(queueItemId);

                    if (queueItem == null)
                        return;

                    await logger.DebugAsync($"Begin processing group membership removal for customer {queueItem.CustomerId}");

                    var customer = await customerService.GetCustomerByIdAsync(queueItem.CustomerId);

                    if (customer != null)
                    {
                        var answerMembership =
                            await nexportService.GetNexportSupplementalInfoAnswerMembership(queueItem.NexportMembershipId);

                        if (answerMembership != null)
                        {
                            await nexportService.DeleteNexportSupplementalInfoAnswerMembership(answerMembership);
                        }

                        try
                        {
                            var removalMembership = await nexportService.RemoveNexportMembershipsAsync(
                                new List<Guid>(1)
                                {
                                    queueItem.NexportMembershipId
                                });

                            if (removalMembership.Count == 0)
                                throw new Exception("Failed to remove membership in Nexport");

                            await customerActivityService.InsertActivityAsync(customer,
                                NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                $"Successfully removed membership [{queueItem.NexportMembershipId}] in Nexport.");
                        }
                        catch (Exception ex)
                        {
                            await customerActivityService.InsertActivityAsync(customer,
                                NexportDefaults
                                    .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                $"Cannot remove membership [{queueItem.NexportMembershipId}] in Nexport." +
                                $" Error: {(ex as ApiException)?.Message}");
                        }
                    }

                    await nexportService.DeleteNexportGroupMembershipRemovalQueueItem(queueItem);

                    await logger.InformationAsync($"Group membership removal queue item {queueItemId} has been processed and removed!");
                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync($"Cannot process the NexportGroupMembershipRemovalQueue item with Id {queueItemId}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Cannot process the NexportGroupMembershipRemovalQueue", ex);
        }
    }
}