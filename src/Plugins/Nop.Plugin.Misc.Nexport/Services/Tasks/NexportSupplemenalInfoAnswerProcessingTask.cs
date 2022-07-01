using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexportApi.Client;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportSupplementalInfoAnswerProcessingTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IRepository<NexportSupplementalInfoAnswerProcessingQueueItem> _nexportSupplementalInfoAnswerProcessingQueueRepository;
        private readonly NexportService _nexportService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ISettingService _settingService;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize;

        public NexportSupplementalInfoAnswerProcessingTask(
            IWidgetPluginManager widgetPluginManager,
            ILogger logger,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            IRepository<NexportSupplementalInfoAnswerProcessingQueueItem> nexportSupplementalInfoAnswerProcessingQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _logger = logger;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _nexportSupplementalInfoAnswerProcessingQueueRepository = nexportSupplementalInfoAnswerProcessingQueueRepository;
            _nexportService = nexportService;
        }

        public async Task ExecuteAsync()
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync("Misc.Nexport"))
                return;

            try
            {
                _batchSize = await _settingService.GetSettingByKeyAsync(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                    NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSize);

                var answers = await _nexportSupplementalInfoAnswerProcessingQueueRepository.Table
                    .OrderBy(q => q.UtcDateCreated)
                    .Select(q => q.Id)
                    .Take(_batchSize)
                    .ToListAsync();

                await ProcessNexportSupplementalInfoAnswersAsync(answers);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process Nexport supplemental info answers", ex);
            }
        }

        public async Task ProcessNexportSupplementalInfoAnswersAsync(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var queueItem = await _nexportSupplementalInfoAnswerProcessingQueueRepository.GetByIdAsync(queueItemId);

                        if (queueItem == null)
                            return;

                        _logger.DebugAsync($"Begin processing supplemental info answer for answer {queueItem.AnswerId}");

                        var answer = await _nexportService.GetNexportSupplementalInfoAnswerById(queueItem.AnswerId);

                        if (answer != null)
                        {
                            var option = await _nexportService.GetNexportSupplementalInfoOptionById(answer.OptionId);
                            if (option != null)
                            {
                                var customerMapping = await _nexportService.FindUserMappingByCustomerId(answer.CustomerId);
                                if (customerMapping != null)
                                {
                                    var customer = await _customerService.GetCustomerByIdAsync(customerMapping.NopUserId);
                                    if (customer != null)
                                    {
                                        var groupAssociations =
                                            await _nexportService.GetNexportSupplementalInfoOptionGroupAssociations(option.Id, true);

                                        foreach (var groupAssociation in groupAssociations)
                                        {
                                            try
                                            {
                                                var newMemberShipInfo = await _nexportService.AddNexportMembershipsAsync(customerMapping.NexportUserId, new List<Guid>(1)
                                                {
                                                    groupAssociation.NexportGroupId
                                                });

                                                if (newMemberShipInfo.Count == 0)
                                                    throw new Exception("Failed to create membership in Nexport");

                                                await _nexportService.InsertNexportSupplementalInfoAnswerMembership(
                                                    new NexportSupplementalInfoAnswerMembership
                                                    {
                                                        AnswerId = answer.Id,
                                                        NexportMembershipId = newMemberShipInfo[0].MembershipId
                                                    });

                                                await _customerActivityService.InsertActivityAsync(customer,
                                                    NexportDefaults
                                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                                    $"Successfully created membership for the group {groupAssociation.NexportGroupName} ({groupAssociation.NexportGroupShortName}) [Id: {groupAssociation.NexportGroupId}] in Nexport.");
                                            }
                                            catch (Exception ex)
                                            {
                                                await _customerActivityService.InsertActivityAsync(customer,
                                                    NexportDefaults
                                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                                    $"Cannot create membership for the group {groupAssociation.NexportGroupName} ({groupAssociation.NexportGroupShortName}) [Id: {groupAssociation.NexportGroupId}] in Nexport." +
                                                    $" Error: {(ex as ApiException).Message}");
                                            }
                                        }

                                        answer.Status = NexportSupplementalInfoAnswerStatus.Processed;
                                        answer.UtcDateProcessed = DateTime.UtcNow;

                                        await _nexportService.UpdateNexportSupplementalInfoAnswer(answer);
                                    }
                                }
                            }
                        }

                        await _nexportService.DeleteNexportSupplementalInfoAnswerProcessingQueueItem(queueItem);

                        await _logger.InformationAsync($"Supplemental info answer processing queue item {queueItemId} has been processed and removed!");
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Cannot process the NexportSupplementalInfoAnswerQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Cannot process the NexportSupplementalInfoAnswerQueue", ex);
            }
        }
    }
}
