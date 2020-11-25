using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Client;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
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

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                _batchSize = _settingService.GetSettingByKey(
                    NexportDefaults.NexportGroupMembershipRemovalTaskBatchSizeSettingKey,
                    NexportDefaults.NexportGroupMembershipRemovalTaskBatchSize);

                var answers = (_nexportGroupMembershipRemovalQueueRepository.Table.OrderBy(q => q.UtcDateCreated)
                    .Select(q => q.Id)).Take(_batchSize).ToList();

                ProcessNexportGroupMembershipRemoval(answers);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process Nexport group membership removal", ex);
            }
        }

        public void ProcessNexportGroupMembershipRemoval(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var queueItem = _nexportGroupMembershipRemovalQueueRepository.GetById(queueItemId);

                        if (queueItem == null)
                            return;

                        _logger.Debug($"Begin processing group membership removal for customer {queueItem.CustomerId}");

                        var customer = _customerService.GetCustomerById(queueItem.CustomerId);

                        if (customer != null)
                        {
                            var answerMembership =
                                _nexportService.GetNexportSupplementalInfoAnswerMembership(
                                    queueItem.NexportMembershipId);

                            if (answerMembership != null)
                            {
                                _nexportService.DeleteNexportSupplementalInfoAnswerMembership(answerMembership);
                            }

                            try
                            {
                                var removalMembership = _nexportService.RemoveNexportMemberships(new List<Guid>(1)
                                {
                                    queueItem.NexportMembershipId
                                });

                                if (removalMembership.Count == 0)
                                    throw new Exception("Failed to remove membership in Nexport");

                                _customerActivityService.InsertActivity(customer,
                                    NexportDefaults.NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                    $"Successfully removed membership [{queueItem.NexportMembershipId}] in Nexport.");
                            }
                            catch (Exception ex)
                            {
                                _customerActivityService.InsertActivity(customer,
                                    NexportDefaults
                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                    $"Cannot remove membership [{queueItem.NexportMembershipId}] in Nexport." +
                                    $" Error: {(ex as ApiException)?.Message}");
                            }
                        }

                        _nexportService.DeleteNexportGroupMembershipRemovalQueueItem(queueItem);

                        _logger.Information($"Group membership removal queue item {queueItemId} has been processed and removed!");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportGroupMembershipRemovalQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot process the NexportGroupMembershipRemovalQueue", ex);
            }
        }
    }
}
