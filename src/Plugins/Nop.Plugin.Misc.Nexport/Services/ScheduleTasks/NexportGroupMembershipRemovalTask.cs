using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexportApi.Client;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleTasks
{
    public class NexportGroupMembershipRemovalTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportGroupMembershipRemovalQueueItem> _nexportGroupMembershipRemovalQueueRepository;
        private readonly NexportService _nexportService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ISettingService _settingService;

        private int _batchSize;

        public NexportGroupMembershipRemovalTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            ISettingService settingService,
            IRepository<NexportGroupMembershipRemovalQueueItem> nexportGroupMembershipRemovalQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _settingService = settingService;
            _nexportGroupMembershipRemovalQueueRepository = nexportGroupMembershipRemovalQueueRepository;
            _nexportService = nexportService;
        }

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(
                    NexportDefaults.NexportGroupMembershipRemovalTaskBatchSizeSettingKey,
                    NexportDefaults.NexportGroupMembershipRemovalTaskBatchSize);

                var answers = await _nexportGroupMembershipRemovalQueueRepository.Table
                    .OrderBy(q => q.UtcDateCreated)
                    .Select(q => q.Id)
                    .Take(_batchSize)
                    .ToListAsync();

                await ProcessNexportGroupMembershipRemovalAsync(answers);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process Nexport group membership removal", ex);
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
                        var queueItem = await _nexportGroupMembershipRemovalQueueRepository.GetByIdAsync(queueItemId);

                        if (queueItem == null)
                            return;

                        await _logger.DebugAsync($"Begin processing group membership removal for customer {queueItem.CustomerId}");

                        var customer = await _customerService.GetCustomerByIdAsync(queueItem.CustomerId);

                        if (customer != null)
                        {
                            var answerMembership =
                                await _nexportService.GetNexportSupplementalInfoAnswerMembership(queueItem.NexportMembershipId);

                            if (answerMembership != null)
                            {
                                await _nexportService.DeleteNexportSupplementalInfoAnswerMembership(answerMembership);
                            }

                            try
                            {
                                var removalMembership = await _nexportService.RemoveNexportMembershipsAsync(
                                    new List<Guid>(1)
                                    {
                                        queueItem.NexportMembershipId
                                    });

                                if (removalMembership.Count == 0)
                                    throw new Exception("Failed to remove membership in Nexport");

                                await _customerActivityService.InsertActivityAsync(customer,
                                    NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                    $"Successfully removed membership [{queueItem.NexportMembershipId}] in Nexport.");
                            }
                            catch (Exception ex)
                            {
                                await _customerActivityService.InsertActivityAsync(customer,
                                    NexportDefaults
                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                    $"Cannot remove membership [{queueItem.NexportMembershipId}] in Nexport." +
                                    $" Error: {(ex as ApiException)?.Message}");
                            }
                        }

                        await _nexportService.DeleteNexportGroupMembershipRemovalQueueItem(queueItem);

                        await _logger.InformationAsync($"Group membership removal queue item {queueItemId} has been processed and removed!");
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process the NexportGroupMembershipRemovalQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot process the NexportGroupMembershipRemovalQueue", ex);
            }
        }
    }
}
