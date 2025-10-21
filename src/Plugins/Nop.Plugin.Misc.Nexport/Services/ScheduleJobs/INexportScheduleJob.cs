namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public interface INexportScheduleJob
{
    string JobName { get; set; }

    long Interval { get; set; }

    Task ExecuteAsync();
}