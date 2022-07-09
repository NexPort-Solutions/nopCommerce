using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportStoreService : StoreService
    {
        private readonly IRepository<Store> _storeRepository;

        public NexportStoreService(IRepository<Store> storeRepository)
            : base(storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public override async Task<IList<Store>> GetAllStoresAsync()
        {
            var result = await _storeRepository.GetAllAsync(query =>
            {
                return from s in query orderby s.Name, s.DisplayOrder, s.Id select s;
            }, cache => default);

            return result;
        }
    }
}