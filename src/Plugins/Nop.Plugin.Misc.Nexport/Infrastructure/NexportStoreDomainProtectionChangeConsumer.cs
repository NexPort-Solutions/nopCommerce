using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

// Coalesces local store changes into a background refresh signal. Event publication never waits
// for the guard's database read; other app instances refresh independently on their timer.
public sealed class NexportStoreDomainProtectionChangeConsumer(NexportStoreDomainProtection guard) :
    IConsumer<EntityInsertedEvent<Store>>,
    IConsumer<EntityUpdatedEvent<Store>>,
    IConsumer<EntityDeletedEvent<Store>>
{
    public Task HandleEventAsync(EntityInsertedEvent<Store> eventMessage) => RequestRefresh();

    public Task HandleEventAsync(EntityUpdatedEvent<Store> eventMessage) => RequestRefresh();

    public Task HandleEventAsync(EntityDeletedEvent<Store> eventMessage) => RequestRefresh();

    private Task RequestRefresh()
    {
        guard.RequestRefresh();
        return Task.CompletedTask;
    }
}