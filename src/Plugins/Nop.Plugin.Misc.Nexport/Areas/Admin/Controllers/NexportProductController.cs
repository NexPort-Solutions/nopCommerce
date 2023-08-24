using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers
{
    public class NexportProductController : BaseAdminController
    {
        #region Fields
        private readonly IStoreService _storeService;
        private readonly NexportService _nexportService;

        public NexportProductController(IStoreService storeService, NexportService nexportService)
        {
            _storeService = storeService;
            _nexportService = nexportService;
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            var stores = await _storeService.GetAllStoresAsync();
            var list = stores.SelectManyAwait(storeToProducts);
            var products = await list.Select(mappingToJQueryObject)
                .Where(product => product.Label.Contains(term))
                .Take(10)
                .ToListAsync();
            return Json(products);

            async Task<IEnumerable<NexportProductMapping>> storeToProducts(Store store) => (await _nexportService.GetProductMappingsByStoreId(store.Id));
            JQueryObject mappingToJQueryObject(NexportProductMapping mapping) => new(mapping.DisplayName, mapping.NopProductId.ToString());
        }
    }
}
