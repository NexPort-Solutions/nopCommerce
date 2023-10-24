using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using static Nop.Plugin.Misc.Nexport.Defaults;
using static Nop.Plugin.Misc.Nexport.LogType;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks;

public class GroupMembershipRemovalTask : IScheduleTask
{
    private readonly ILogger _logger;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IUserMappingService _userMapping;
    private readonly ICustomerService _customer;
    private readonly ICustomerActivityService _customerActivity;
    private readonly ISettingService _setting;
    private readonly IMembershipService _membership;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly IGroupService _group;
    private int _batchSize;
    private readonly IRepository<GroupMembershipRemovalQueueItem> _groupMembershipRemovalQueues;

    public GroupMembershipRemovalTask(
        IWidgetPluginManager widgetPluginManager,
        ILogger logger,
        ICustomerService customer,
        ICustomerActivityService customerActivity,
        ISettingService setting,
        IUserMappingService userMapping,
        IMembershipService membership,
        ISupplementalInfoService supplementalInfo,
        IGroupService group,
        IRepository<GroupMembershipRemovalQueueItem> groupMembershipRemovalQueues)
    {
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
        _customer = customer;
        _customerActivity = customerActivity;
        _setting = setting;
        _userMapping = userMapping;
        _membership = membership;
        _supplementalInfo = supplementalInfo;
        _group = group;
        _groupMembershipRemovalQueues = groupMembershipRemovalQueues;
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
                GroupMembershipRemovalTaskBatchSizeSettingKey,
                GroupMembershipRemovalTaskBatchSize);
            var answers = await _groupMembershipRemovalQueues.Table
                .OrderBy(queueItem => queueItem.UtcDateCreated)
                .Select(queueItem => queueItem.Id)
                .Take(_batchSize)
                .ToListAsync();
            await ProcessGroupMembershipRemovalAsync(answers);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Cannot process NexPort group membership removal", exception);
        }
    }

    public async Task ProcessGroupMembershipRemovalAsync(IList<int> queueItemIds)
    {
        try
        {
            await foreach (var queueItem in queueItemIds.SelectAwait(async id => await _groupMembershipRemovalQueues.GetByIdAsync(id)).WhereNotNull())
            {
                await _logger.DebugAsync($"Begin processing group membership removal for customer {queueItem.CustomerId}");
                if (await _customer.GetCustomerByIdAsync(queueItem.CustomerId) is { } customer)
                {
                    if (await _supplementalInfo.GetSupplementalInfoAnswerMembership(queueItem.MembershipId) is { } answerMembership)
                    {
                        await _supplementalInfo.DeleteSupplementalInfoAnswerMembership(answerMembership);
                    }
                    var result = await _membership.RemoveMemberships(new List<Guid>(1) { queueItem.MembershipId }) switch
                    {
                        [] => $"Failed to remove membership [{queueItem.MembershipId}] in NexPort.",
                        _ => $"Successfully removed membership [{queueItem.MembershipId}] in NexPort."
                    };
                    await _customerActivity.InsertActivityAsync(customer, PROCESSING_SUPPLEMENTAL_INFO_GROUP_ASSOCIATIONS, result);
                }
                await _group.DeleteGroupMembershipRemovalQueueItem(queueItem);
                await _logger.InformationAsync($"Group membership removal queue id {queueItem.Id} has been processed and removed!");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"Failed to process the {nameof(GroupMembershipRemovalQueueItem)}s.", exception);
        }
    }
}
