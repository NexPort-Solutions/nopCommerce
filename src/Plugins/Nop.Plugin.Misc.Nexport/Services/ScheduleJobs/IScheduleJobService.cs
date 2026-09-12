using Hangfire.Storage;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public interface IScheduleJobService
{
    Task InitializeScheduleJobsAsync();

    Task TriggerJobAsync(string jobId);

    Task RemoveJobAsync(string jobId);

    List<RecurringJobDto> GetAvailableScheduleJobs();
}
