using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;
using Nop.Plugin.Misc.Nexport.Domain.Wholesale;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportWholesaleService
{
    Task<PlaceOrderResult> PlaceWholesaleOrderAsync(ProcessPaymentRequest processPaymentRequest, List<ShoppingCartItem> shoppingCartItems);

    Task<IPagedList<NexportFundingPool>> GetAllFundingPoolsPagination(
        string name, string code, string description,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

    Task<IList<NexportFundingPool>> GetFundingPools();

    Task<NexportFundingPool> GetFundingPoolById(int id);

    Task<IList<NexportFundingPool>> GetFundingPoolByIds(int[] fundingPoolIds);

    Task InsertFundingPool(NexportFundingPool nexportFundingPool);

    Task DeleteFundingPool(NexportFundingPool nexportFundingPool);

    Task DeleteFundingPools(IList<NexportFundingPool> fundingPools);

    Task UpdateFundingPool(NexportFundingPool nexportFundingPool);

    Task<IPagedList<WholesaleOrderProduct>> SearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null,
        bool searchNexportProducts = false);
}