using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Caching;

namespace Nop.Plugin.Misc.Nexport.Services.Caching;

public class NexportProductGroupMembershipMappingCacheEventConsumer : CacheEventConsumer<NexportProductGroupMembershipMapping>
{
    protected override async Task ClearCacheAsync(NexportProductGroupMembershipMapping entity, EntityEventType entityEventType)
    {
        await RemoveByPrefixAsync(NexportIntegrationDefaults.GroupMembershipMappingsByNexportProductMappingIdPrefix, entity);

        await base.ClearCacheAsync(entity, entityEventType);
    }
}
