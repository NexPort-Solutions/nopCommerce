using Nop.Core.Caching;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public static class NexportIntegrationDefaults
    {
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
        public static CacheKey ProductMappingCatalogAllByCatalogIdCacheKey =>
            new CacheKey("Nop.nexport.mapping.catalog.allbycatalogid-{0}-{1}-{2}-{3}-{4}-{5}");

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
        public static CacheKey ProductMappingSectionAllBySectionIdCacheKey => new CacheKey("Nop.nexport.mapping.section.allbysectionid-{0}-{1}-{2}-{3}-{4}-{5}");

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
        public static CacheKey ProductMappingTrainingPlanAllByTrainingPlanIdCacheKey => new CacheKey("Nop.nexport.mapping.trainingplan.allbytrainingplanid-{0}-{1}-{2}-{3}-{4}-{5}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : comma separated list of customer roles
        /// {2} : show hidden records?
        /// {3} : show all
        /// </remarks>
        public static CacheKey ProductMappingsAllCacheKey => new CacheKey("Nop.nexport.mapping.all-{0}-{1}-{2}-{3}");

        public static CacheKey ProductMappingPatternCacheKey => new CacheKey("Nop.nexport.mapping.");

        public static CacheKey ProductGroupMembershipMappingsAllCacheKey => new CacheKey("Nop.nexport.groupmembership.mapping.all-{0}-{1}-{2}-{3}");

        public static CacheKey ProductGroupMembershipMappingPatternCacheKey => new CacheKey("Nop.nexport.groupmembership.mapping.");

        public static CacheKey ProductStoreMappingsAllCacheKey => new CacheKey("Nop.nexport.mapping.store.all");

        public static CacheKey ProductStoreMappingPatternCacheKey => new CacheKey("Nop.nexport.mapping.store.");

        public static CacheKey UserMappingPatternCacheKey => new CacheKey("Nop.nexport.user.mapping.");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// </remarks>
        public static CacheKey SupplementalInfoQuestionAllCacheKey => new CacheKey("Nop.nexport.supplementalinfo.question.all-{0}-{1}");

        public static CacheKey SupplementalInfoQuestionAllNoPaginationCacheKey => new CacheKey("Nop.nexport.supplementalinfo.question.all");

        public static CacheKey SupplementalInfoQuestionPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.question.");

        public static CacheKey SupplementalInfoOptionPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.option.");

        public static CacheKey SupplementalInfoQuestionMappingPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.question.mapping.");

        public static CacheKey SupplementalInfoOptionGroupAssociationsAllCacheKey => new CacheKey("Nop.nexport.supplementalinfo.option.groupassociation.all-{0}");

        public static CacheKey SupplementalInfoOptionGroupAssociationPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.option.groupassociation.");

        public static CacheKey SupplementalInfoAnswerAllCacheKey => new CacheKey("Nop.nexport.supplementalinfo.answer.all");

        public static CacheKey SupplementalInfoAnswerPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.answer.");

        public static CacheKey SupplementalInfoAnswerMembershipPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.answermembership.");

        public static CacheKey SupplementalInfoRequiredPatternCacheKey => new CacheKey("Nop.nexport.supplementalinfo.required.");

        public static CacheKey RegistrationFieldAllCacheKey => new CacheKey("Nop.nexport.registrationfield.all");

        public static CacheKey RegistrationFieldOptionAllCacheKey => new CacheKey("Nop.nexport.registrationfield.option.all");

        public static CacheKey RegistrationFieldPatternCacheKey => new CacheKey("Nop.nexport.registrationfield.");

        public static CacheKey RegistrationFieldCategoryAllCacheKey => new CacheKey("Nop.nexport.registrationfield.category.all");

        public static CacheKey RegistrationFieldCategoryPatternCacheKey => new CacheKey("Nop.nexport.registrationfield.category.");

        public static CacheKey RegistrationFieldAnswerAllCacheKey => new CacheKey("Nop.nexport.registrationfield.answer.all");

        public static CacheKey RegistrationFieldAnswerPatternCacheKey => new CacheKey("Nop.nexport.registrationfield.answer.");
    }
}
