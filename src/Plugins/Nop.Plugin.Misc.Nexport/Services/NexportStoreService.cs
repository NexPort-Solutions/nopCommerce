using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportStoreService : StoreService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Store> _storeRepository;
        private readonly IStaticCacheManager _cacheManager;

        public NexportStoreService(
            IEventPublisher eventPublisher, 
            IRepository<Store> storeRepository, 
            IStaticCacheManager cacheManager) 
            : base(eventPublisher, storeRepository, cacheManager)
        {
            _eventPublisher = eventPublisher;
            _storeRepository = storeRepository;
            _cacheManager = cacheManager;
        }

        public override IList<Store> GetAllStores(bool loadCacheableCopy = true)
        {
            IList<Store> LoadStoresFunc()
            {
                var query = from s in _storeRepository.Table orderby s.Name, s.DisplayOrder, s.Id select s;
                return query.ToList();
            }

            if (loadCacheableCopy)
            {
                //cacheable copy
                return _cacheManager.Get(NopStoreDefaults.StoresAllCacheKey, () =>
                {
                    var result = new List<Store>();
                    foreach (var store in LoadStoresFunc())
                        result.Add(new StoreForCaching(store));
                    return result;
                });
            }

            return LoadStoresFunc();
        }
    }
}