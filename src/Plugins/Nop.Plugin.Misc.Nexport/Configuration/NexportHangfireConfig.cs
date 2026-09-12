namespace Nop.Plugin.Misc.Nexport.Configuration;

public sealed class NexportHangfireConfig
{
    public const string SectionName = "NexportHangfire";

    public const int DefaultWorkerCount = 5;

    public const int DefaultSchedulePollingIntervalSeconds = 5;

    public int WorkerCount { get; set; } = DefaultWorkerCount;

    public int SchedulePollingIntervalSeconds { get; set; } = DefaultSchedulePollingIntervalSeconds;
}
