using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nop.Plugin.Misc.Nexport.Services.ScheduleJobs;

public sealed class NexportRecurringJobStartupService(
    IServiceScopeFactory serviceScopeFactory,
    Microsoft.Extensions.Logging.ILogger<NexportRecurringJobStartupService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var scheduleJobService = scope.ServiceProvider.GetRequiredService<IScheduleJobService>();
            await scheduleJobService.InitializeScheduleJobsAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to initialize Nexport recurring Hangfire jobs");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
