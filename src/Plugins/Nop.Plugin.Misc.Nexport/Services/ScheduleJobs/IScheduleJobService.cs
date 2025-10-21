using Hangfire.Storage;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public interface IScheduleJobService
{
    Task InitializeScheduleJobs();

    Task RunJob(string jobId);

    Task RemoveJob(string jobId);

    List<RecurringJobDto> GetAvailableScheduleJobs();
}