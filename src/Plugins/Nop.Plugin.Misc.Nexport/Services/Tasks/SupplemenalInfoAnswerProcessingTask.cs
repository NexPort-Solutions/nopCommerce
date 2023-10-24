using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using static Nop.Plugin.Misc.Nexport.LogType;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class SupplementalInfoAnswerProcessingTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IRepository<AnswerProcessingQueueItem> _supplementalInfoAnswerProcessingQueues;
    private readonly IUserMappingService _userMapping;
    private readonly ICustomerService _customer;
    private readonly ICustomerActivityService _customerActivity;
    private readonly ISettingService _setting;
    private readonly IMembershipService _membershipService;
    private readonly ISupplementalInfoService _supplementalInfo;
    private int _batchSize;

    public SupplementalInfoAnswerProcessingTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        ICustomerService customerService,
        ICustomerActivityService customerActivityService,
        ISettingService settingService,
        IRepository<AnswerProcessingQueueItem> supplementalInfoAnswerProcessingQueueRepository,
        IUserMappingService userMapping,
        IMembershipService membershipService,
        ISupplementalInfoService supplementalInfo)
    {
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _customer = customerService;
        _customerActivity = customerActivityService;
        _setting = settingService;
        _supplementalInfoAnswerProcessingQueues = supplementalInfoAnswerProcessingQueueRepository;
        _userMapping = userMapping;
        _membershipService = membershipService;
        _supplementalInfo = supplementalInfo;
    }

    public async Task ExecuteAsync()
    {
        if (!await _widgetPluginManager.IsPluginActiveAsync(SystemNames.SYSTEM_NAME))
        {
            return;
        }
        try
        {
            _batchSize = await _setting.GetSettingByKeyAsync(
                Defaults.SupplementalInfoAnswerProcessingTaskBatchSizeSettingKey,
                Defaults.SupplementalInfoAnswerProcessingTaskBatchSize);
            var answers = await _supplementalInfoAnswerProcessingQueues.Table
                .OrderBy(queueItem => queueItem.UtcDateCreated)
                .Select(queueItem => queueItem.Id)
                .Take(_batchSize)
                .ToListAsync();
            await ProcessSupplementalInfoAnswersAsync(answers);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot process NexPort supplemental info answers", exception);
        }
    }

    public async Task ProcessSupplementalInfoAnswersAsync(IList<int> queueItemIds)
    {
        foreach (var queueItemId in queueItemIds)
        {
            try
            {
                var queueItem = await _supplementalInfoAnswerProcessingQueues.GetByIdAsync(queueItemId);
                if (queueItem is null)
                {
                    continue;
                }
                await _logger.DebugAsync($"Begin processing supplemental info answer for answer {queueItem.AnswerId}");
                if (await _supplementalInfo.GetSupplementalInfoAnswerById(queueItem.AnswerId) is not { } answer
                    || await _supplementalInfo.GetSupplementalInfoOptionById(answer.OptionId) is not { } option
                    || await _userMapping.FindByCustomerId(answer.CustomerId) is not { } customerMapping
                    || await _customer.GetCustomerByIdAsync(customerMapping.NopUserId) is not { } customer)
                {
                    await _supplementalInfo.DeleteSupplementalInfoAnswerProcessingQueueItem(queueItem);
                    await _logger.InformationAsync($"Supplemental info answer processing queue item {queueItemId} has been processed and removed!");
                    continue;
                }
                var groupAssociations = await _supplementalInfo.GetSupplementalInfoOptionGroupAssociations(option.Id, true);
                foreach (var groupAssociation in groupAssociations)
                {
                    var name = groupAssociation.GroupName;
                    var shortName = groupAssociation.GroupShortName;
                    var id = groupAssociation.GroupId;
                    try
                    {
                        var newMemberShipInfo = await _membershipService.AddMemberships(customerMapping.UserId, new List<Guid>(1) { id });
                        if (newMemberShipInfo.Count is 0)
                        {
                            var errorMessage = $"Cannot create membership for the group {name} ({shortName}) [GroupGuid: {id}] in NexPort. Error: Failed to create membership in NexPort.";
                            await _customerActivity.InsertActivityAsync(customer, PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS, errorMessage);
                            continue;
                        }
                        var answerMembership = new AnswerMembership
                        {
                            AnswerId = answer.Id,
                            MembershipId = newMemberShipInfo[0].MembershipId,
                        };
                        await _supplementalInfo.InsertSupplementalInfoAnswerMembership(answerMembership);
                        var comment = $"Successfully created membership for the group {name} ({shortName}) [GroupGuid: {id}] in NexPort.";
                        await _customerActivity.InsertActivityAsync(customer, PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS, comment);
                    }
                    catch (Exception exception)
                    {
                        var comment = $"Cannot create membership for the group {name} ({shortName}) [GroupGuid: {id}] in NexPort. Error: {exception.Message}";
                        await _customerActivity.InsertActivityAsync(customer, PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS, comment);
                    }
                }
                answer.Status = Answer.AnswerStatus.Processed;
                answer.UtcDateProcessed = DateTime.UtcNow;
                await _supplementalInfo.UpdateSupplementalInfoAnswer(answer);
                await _supplementalInfo.DeleteSupplementalInfoAnswerProcessingQueueItem(queueItem);
                await _logger.InformationAsync($"Supplemental info answer processing queue item {queueItemId} has been processed and removed!");
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync($"Cannot process the SupplementalInfoAnswerQueue item with GroupGuid {queueItemId}", exception);
            }
        }
    }
}
