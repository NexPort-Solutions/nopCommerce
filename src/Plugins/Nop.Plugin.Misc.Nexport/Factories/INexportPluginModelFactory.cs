using System;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Plugin.Misc.Nexport.Models;

namespace Nop.Plugin.Misc.Nexport.Factories
{
    public partial interface INexportPluginModelFactory
    {
        NexportProductMappingSearchModel PrepareNexportProductMappingSearchModel(
            NexportProductMappingSearchModel searchModel);

        MapProductToNexportProductListModel PrepareMapProductToNexportProductListModel(
            NexportProductMappingSearchModel searchModel);

        NexportProductMappingListModel PrepareNexportProductMappingListModel(
            NexportProductMappingSearchModel searchModel, Guid nexportProductId, NexportProductTypeEnum nexportProductType);

        NexportUserMappingModel PrepareNexportUserMappingModel(Customer customer);

        NexportCatalogListModel PrepareNexportCatalogListModel(NexportCatalogSearchModel searchModel);

        NexportSyllabusListModel PrepareNexportSyllabusListMode(NexportSyllabusListSearchModel searchModel);
    }
}
