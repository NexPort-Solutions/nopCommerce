using System.Diagnostics.Metrics;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Nop.Plugin.Misc.Nexport.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipConcurrentExecutionAttribute : JobFilterAttribute, IServerFilter
{
    private const string DistributedLockItemKey = "Nexport.SkipConcurrentExecution.DistributedLock";

    private static readonly Meter Meter = new("Nop.Plugin.Misc.Nexport.Hangfire", "1.0.0");

    private static readonly Counter<long> SkippedExecutions =
        Meter.CreateCounter<long>("nexport.hangfire.executions.skipped");

    public void OnPerforming(PerformingContext context)
    {
        var job = context.BackgroundJob.Job;
        var resource = $"nexport:{job.Type.FullName}.{job.Method.Name}";

        try
        {
            context.Items[DistributedLockItemKey] = context.Connection.AcquireDistributedLock(resource, TimeSpan.Zero);
        }
        catch (DistributedLockTimeoutException)
        {
            context.Canceled = true;
            SkippedExecutions.Add(1,
                new KeyValuePair<string, object?>("job", job.Type.Name),
                new KeyValuePair<string, object?>("worker", context.ServerId));
        }
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue(DistributedLockItemKey, out var value) && value is IDisposable distributedLock)
            distributedLock.Dispose();
    }
}
