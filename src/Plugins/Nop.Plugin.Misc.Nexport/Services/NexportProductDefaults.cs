﻿using System;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public static partial class NexportProductDefaults
    {
        public static string GetProductMappingCatalogAllByCatalogIdCacheKey(bool showHidden, Guid catalogId,
            int pageIndex, int pageSize, int customerId, int storeId)
        {
            return string.Format(ProductMappingCatalogAllByCatalogIdCacheKey, showHidden, catalogId, pageIndex, pageSize, customerId, storeId);
        }

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : catalog ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        public static string ProductMappingCatalogAllByCatalogIdCacheKey => "Nop.nexport.mapping.catalog.allbycatalogid-{0}-{1}-{2}-{3}-{4}-{5}";

        public static string GetProductMappingSectionAllBySectionIdCacheKey(bool showHidden, Guid sectionId,
            int pageIndex, int pageSize, int customerId, int storeId)
        {
            return string.Format(ProductMappingSectionAllBySectionIdCacheKey, showHidden, sectionId, pageIndex, pageSize, customerId, storeId);
        }

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : section ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        public static string ProductMappingSectionAllBySectionIdCacheKey => "Nop.nexport.mapping.section.allbysectionid-{0}-{1}-{2}-{3}-{4}-{5}";

        public static string GetProductMappingTrainingPlanAllByTrainingPlanIdCacheKey(bool showHidden, Guid trainingPlanId,
            int pageIndex, int pageSize, int customerId, int storeId)
        {
            return string.Format(ProductMappingTrainingPlanAllByTrainingPlanIdCacheKey, showHidden, trainingPlanId, pageIndex, pageSize, customerId, storeId);
        }

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : section ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        public static string ProductMappingTrainingPlanAllByTrainingPlanIdCacheKey => "Nop.nexport.mapping.trainingplan.allbytrainingplanid-{0}-{1}-{2}-{3}-{4}-{5}";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : comma separated list of customer roles
        /// {2} : show hidden records?
        /// {3} : show all
        /// </remarks>
        public static string ProductMappingsAllCacheKey => "Nop.nexport.mapping.all-{0}-{1}-{2}-{3}";

        public static string ProductMappingPatternCacheKey => "Nop.nexport.mapping.";

        public static string ProductGroupMembershipMappingsAllCacheKey => "Nop.nexport.groupmembership.mapping.all-{0}-{1}-{2}-{3}";

        public static string ProductGroupMembershipMappingPatternCacheKey => "Nop.nexport.groupmembership.mapping.";

        public static string ProductStoreMappingsAllCacheKey => "Nop.nexport.mapping.store.all";

        public static string ProductStoreMappingPatternCacheKey => "Nop.nexport.mapping.store.";

        public static string UserMappingPatternCacheKey => "Nop.nexport.user.mapping.";
    }
}
