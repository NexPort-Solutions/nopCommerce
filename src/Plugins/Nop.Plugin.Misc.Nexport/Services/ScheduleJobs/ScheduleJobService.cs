using System.Linq.Expressions;
using Hangfire;
using Hangfire.Storage;
using Nop.Plugin.Misc.Nexport.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public class ScheduleJobService(IRecurringJobManager recurringJobManager) : IScheduleJobService
{
    public Task InitializeScheduleJobsAsync()
    {
        AddOrUpdate<NexportOrderProcessingScheduleJob>("NexportOrderProcessing", "*/5 * * * * *", job => job.ExecuteAsync());
        AddOrUpdate<NexportInvoiceResetRedemptionScheduleJob>("NexportInvoiceResetRedemption", "*/5 * * * * *",
            job => job.ExecuteAsync());
        AddOrUpdate<NexportInvoiceRedemptionScheduleJob>("NexportInvoiceRedemption", "*/30 * * * * *", job => job.ExecuteAsync());
        AddOrUpdate<NexportSupplementalInfoAnswerProcessingScheduleJob>("NexportSupplementalInfoAnswerProcessing", "*/30 * * * * *",
            job => job.ExecuteAsync());
        AddOrUpdate<NexportRegistrationFieldSynchronizationScheduleJob>("NexportRegistrationFieldSynchronization", "0 */5 * * * *",
            job => job.ExecuteAsync());
        AddOrUpdate<NexportGroupMembershipRemovalScheduleJob>("NexportGroupMembershipRemoval", "*/30 * * * * *",
            job => job.ExecuteAsync());

        return Task.CompletedTask;
    }

    public Task TriggerJobAsync(string jobId)
    {
        recurringJobManager.Trigger(jobId);
        return Task.CompletedTask;
    }

    public Task RemoveJobAsync(string jobId)
    {
        recurringJobManager.RemoveIfExists(jobId);
        return Task.CompletedTask;
    }

    public List<RecurringJobDto> GetAvailableScheduleJobs()
    {
        return JobStorage.Current.GetConnection().GetRecurringJobs();
    }

    private void AddOrUpdate<T>(string jobId, string cronExpression, Expression<Func<T, Task>> executeExpression)
    {
        recurringJobManager.AddOrUpdate(jobId, executeExpression, cronExpression, new RecurringJobOptions());
    }
}
