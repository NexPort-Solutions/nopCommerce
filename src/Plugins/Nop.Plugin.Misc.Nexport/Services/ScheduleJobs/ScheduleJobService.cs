using System.Reflection;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public class ScheduleJobService(
    IServiceProvider serviceProvider,
    ILogger logger)
    : IScheduleJobService
{
    public async Task InitializeScheduleJobs()
    {
        try
        {
            // Get all the schedule jobs that implement the interface IScheduleTask
            var scheduleJobImplementations = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INexportScheduleJob)));

            foreach (var instanceType in scheduleJobImplementations)
            {
                // Create individual schedule job object instance
                var scheduleTaskObject = (INexportScheduleJob)ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
                var scheduleJobIntervalTimespan = TimeSpan.FromSeconds(scheduleTaskObject.Interval).ConvertToCronExpression();
                RecurringJob.AddOrUpdate(
                    scheduleTaskObject.JobName,
                    () => scheduleTaskObject.ExecuteAsync(),
                    scheduleJobIntervalTimespan);
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync("Unable to initialize schedule jobs!", ex);
            throw;
        }
    }

    public Task RunJob(string jobId)
    {
        return Task.Run(() => RecurringJob.RemoveIfExists(jobId));
    }

    public Task RemoveJob(string jobId)
    {
        return Task.Run(() => RecurringJob.TriggerJob(jobId));
    }

    public List<RecurringJobDto> GetAvailableScheduleJobs()
    {
        return JobStorage.Current.GetConnection().GetRecurringJobs();
    }
}
