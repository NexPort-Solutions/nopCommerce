using Nop.Core.Caching;

namespace Nop.Plugin.Misc.Nexport.Services;

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
        new("Nop.nexport.mapping.catalog.allbycatalogid-{0}-{1}-{2}-{3}-{4}-{5}");

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
    public static CacheKey ProductMappingSectionAllBySectionIdCacheKey => new("Nop.nexport.mapping.section.allbysectionid-{0}-{1}-{2}-{3}-{4}-{5}");

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
    public static CacheKey ProductMappingTrainingPlanAllByTrainingPlanIdCacheKey => new("Nop.nexport.mapping.trainingplan.allbytrainingplanid-{0}-{1}-{2}-{3}-{4}-{5}");

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : current store ID
    /// {1} : comma separated list of customer roles
    /// {2} : show hidden records?
    /// {3} : show all
    /// </remarks>
    public static CacheKey ProductMappingsAllCacheKey => new("Nop.nexport.mapping.all-{0}-{1}-{2}-{3}");

    public static string GroupMembershipMappingsByNexportProductMappingIdPrefix => "Nop.nexport.groupmembership.mapping.{0}";

    /// <summary>
    /// Gets a key for caching group memberships in product mapping
    /// </summary>
    /// <remarks>
    /// {0} : cNexport product mapping ID
    /// </remarks>
    public static CacheKey GroupMembershipMappingsByNexportProductMappingIdCacheKey => new("Nop.nexport.groupmembership.mapping.{0}", GroupMembershipMappingsByNexportProductMappingIdPrefix);

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : page index
    /// {1} : page size
    /// </remarks>
    public static CacheKey SupplementalInfoQuestionAllCacheKey => new("Nop.nexport.supplementalinfo.question.all-{0}-{1}");

    public static CacheKey SupplementalInfoQuestionAllNoPaginationCacheKey => new("Nop.nexport.supplementalinfo.question.all");

    public static CacheKey SupplementalInfoQuestionPatternCacheKey => new("Nop.nexport.supplementalinfo.question.");

    public static CacheKey SupplementalInfoOptionPatternCacheKey => new("Nop.nexport.supplementalinfo.option.");

    public static CacheKey SupplementalInfoQuestionMappingPatternCacheKey => new("Nop.nexport.supplementalinfo.question.mapping.");

    public static CacheKey SupplementalInfoOptionGroupAssociationsAllCacheKey => new("Nop.nexport.supplementalinfo.option.groupassociation.all-{0}");

    public static CacheKey SupplementalInfoOptionGroupAssociationPatternCacheKey => new("Nop.nexport.supplementalinfo.option.groupassociation.");

    public static CacheKey SupplementalInfoAnswerAllCacheKey => new("Nop.nexport.supplementalinfo.answer.all");

    public static CacheKey SupplementalInfoAnswerPatternCacheKey => new("Nop.nexport.supplementalinfo.answer.");

    public static CacheKey SupplementalInfoAnswerMembershipPatternCacheKey => new("Nop.nexport.supplementalinfo.answermembership.");

    public static CacheKey SupplementalInfoRequiredPatternCacheKey => new("Nop.nexport.supplementalinfo.required.");

    public static CacheKey RegistrationFieldAllCacheKey => new("Nop.nexport.registrationfield.all");

    public static CacheKey RegistrationFieldOptionAllCacheKey => new("Nop.nexport.registrationfield.option.all");

    public static CacheKey RegistrationFieldPatternCacheKey => new("Nop.nexport.registrationfield.");

    public static CacheKey RegistrationFieldCategoryAllCacheKey => new("Nop.nexport.registrationfield.category.all");

    public static CacheKey RegistrationFieldCategoryPatternCacheKey => new("Nop.nexport.registrationfield.category.");

    public static CacheKey RegistrationFieldAnswerAllCacheKey => new("Nop.nexport.registrationfield.answer.all");

    public static CacheKey RegistrationFieldAnswerPatternCacheKey => new("Nop.nexport.registrationfield.answer.");
}