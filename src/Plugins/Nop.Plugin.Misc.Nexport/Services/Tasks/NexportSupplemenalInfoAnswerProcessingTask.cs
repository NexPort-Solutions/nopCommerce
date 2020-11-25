using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Client;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Extensions;

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

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                _batchSize = _settingService.GetSettingByKey(NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                    NexportDefaults.NexportSupplementalInfoAnswerProcessingTaskBatchSize);

                var answers = (_nexportSupplementalInfoAnswerProcessingQueueRepository.Table
                    .OrderBy(q => q.UtcDateCreated)
                    .Select(q => q.Id))
                    .Take(_batchSize).ToList();

                ProcessNexportSupplementalInfoAnswers(answers);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process Nexport supplemental info answers", ex);
            }
        }

        public void ProcessNexportSupplementalInfoAnswers(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var queueItem = _nexportSupplementalInfoAnswerProcessingQueueRepository.GetById(queueItemId);

                        if (queueItem == null)
                            return;

                        _logger.Debug($"Begin processing supplemental info answer for answer {queueItem.AnswerId}");

                        var answer = _nexportService.GetNexportSupplementalInfoAnswerById(queueItem.AnswerId);

                        if (answer != null)
                        {
                            var option = _nexportService.GetNexportSupplementalInfoOptionById(answer.OptionId);
                            if (option != null)
                            {
                                var customerMapping = _nexportService.FindUserMappingByCustomerId(answer.CustomerId);
                                if (customerMapping != null)
                                {
                                    var customer = _customerService.GetCustomerById(customerMapping.NopUserId);
                                    if (customer != null)
                                    {
                                        var groupAssociations =
                                            _nexportService.GetNexportSupplementalInfoOptionGroupAssociations(option.Id, true);

                                        foreach (var groupAssociation in groupAssociations)
                                        {
                                            try
                                            {
                                                var newMemberShipInfo = _nexportService.AddNexportMemberships(customerMapping.NexportUserId, new List<Guid>(1)
                                                {
                                                    groupAssociation.NexportGroupId
                                                });

                                                if (newMemberShipInfo.Count == 0)
                                                    throw new Exception("Failed to create membership in Nexport");

                                                _nexportService.InsertNexportSupplementalInfoAnswerMembership(
                                                    new NexportSupplementalInfoAnswerMembership
                                                    {
                                                        AnswerId = answer.Id,
                                                        NexportMembershipId = newMemberShipInfo[0].MembershipId
                                                    });

                                                _customerActivityService.InsertActivity(customer,
                                                    NexportDefaults
                                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                                    $"Successfully created membership for the group {groupAssociation.NexportGroupName} ({groupAssociation.NexportGroupShortName}) [Id: {groupAssociation.NexportGroupId}] in Nexport.");
                                            }
                                            catch (Exception ex)
                                            {
                                                _customerActivityService.InsertActivity(customer,
                                                    NexportDefaults
                                                        .NEXPORT_PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS_ACTIVITY_LOG_TYPE,
                                                    $"Cannot create membership for the group {groupAssociation.NexportGroupName} ({groupAssociation.NexportGroupShortName}) [Id: {groupAssociation.NexportGroupId}] in Nexport." +
                                                    $" Error: {(ex as ApiException).Message}");
                                            }
                                        }

                                        answer.Status = NexportSupplementalInfoAnswerStatus.Processed;
                                        answer.UtcDateProcessed = DateTime.UtcNow;

                                        _nexportService.UpdateNexportSupplementalInfoAnswer(answer);
                                    }
                                }
                            }
                        }

                        _nexportService.DeleteNexportSupplementalInfoAnswerProcessingQueueItem(queueItem);

                        _logger.Information($"Supplemental info answer processing queue item {queueItemId} has been processed and removed!");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportSupplementalInfoAnswerQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot process the NexportSupplementalInfoAnswerQueue", ex);
            }
        }
    }
}
