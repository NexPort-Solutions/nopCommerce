using CachingKey = Nop.Core.Caching.CacheKey;

namespace Nop.Plugin.Misc.Nexport.Services;

public static class CacheKey
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
    public static CachingKey ProductMappingCatalogAllByCatalogId => new("Nop.nexport.mapping.catalog.allbycatalogid-{0}-{1}-{2}-{3}-{4}-{5}");

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
    public static CachingKey ProductMappingSectionAllBySectionId => new("Nop.nexport.mapping.section.allbysectionid-{0}-{1}-{2}-{3}-{4}-{5}");

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
    public static CachingKey ProductMappingTrainingPlanAllByTrainingPlanId => new("Nop.nexport.mapping.trainingplan.allbytrainingplanid-{0}-{1}-{2}-{3}-{4}-{5}");

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : current store ID
    /// {1} : comma separated list of customer roles
    /// {2} : show hidden records?
    /// {3} : show all
    /// </remarks>
    public static CachingKey ProductMappingsAll => new("Nop.nexport.mapping.all-{0}-{1}-{2}-{3}");
    public static CachingKey ProductMappingPattern => new("Nop.nexport.mapping.");
    public static CachingKey ProductGroupMembershipMappingsAll => new("Nop.nexport.groupmembership.mapping.all-{0}-{1}-{2}-{3}");
    public static CachingKey ProductGroupMembershipMappingPattern => new("Nop.nexport.groupmembership.mapping.");
    public static CachingKey ProductStoreMappingsAll => new("Nop.nexport.mapping.store.all");
    public static CachingKey ProductStoreMappingPattern => new("Nop.nexport.mapping.store.");
    public static CachingKey UserMappingPattern => new("Nop.nexport.user.mapping.");

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : page index
    /// {1} : page size
    /// </remarks>
    public static CachingKey SupplementalInfoQuestionAll => new("Nop.nexport.supplementalinfo.question.all-{0}-{1}");
    public static CachingKey SupplementalInfoQuestionAllNoPagination => new("Nop.nexport.supplementalinfo.question.all");
    public static CachingKey SupplementalInfoQuestionPattern => new("Nop.nexport.supplementalinfo.question.");
    public static CachingKey SupplementalInfoOptionPattern => new("Nop.nexport.supplementalinfo.option.");
    public static CachingKey SupplementalInfoQuestionMappingPattern => new("Nop.nexport.supplementalinfo.question.mapping.");
    public static CachingKey SupplementalInfoOptionGroupAssociationsAll => new("Nop.nexport.supplementalinfo.option.groupassociation.all-{0}");
    public static CachingKey SupplementalInfoOptionGroupAssociationPattern => new("Nop.nexport.supplementalinfo.option.groupassociation.");
    public static CachingKey SupplementalInfoAnswerAll => new("Nop.nexport.supplementalinfo.answer.all");
    public static CachingKey SupplementalInfoAnswerPattern => new("Nop.nexport.supplementalinfo.answer.");
    public static CachingKey SupplementalInfoAnswerMembershipPattern => new("Nop.nexport.supplementalinfo.answermembership.");
    public static CachingKey SupplementalInfoRequiredPattern => new("Nop.nexport.supplementalinfo.required.");
    public static CachingKey RegistrationFieldAll => new("Nop.nexport.registrationfield.all");
    public static CachingKey RegistrationFieldOptionAll => new("Nop.nexport.registrationfield.option.all");
    public static CachingKey RegistrationFieldPattern => new("Nop.nexport.registrationfield.");
    public static CachingKey RegistrationFieldCategoryAll => new("Nop.nexport.registrationfield.category.all");
    public static CachingKey RegistrationFieldCategoryPattern => new("Nop.nexport.registrationfield.category.");
    public static CachingKey RegistrationFieldAnswerAll => new("Nop.nexport.registrationfield.answer.all");
    public static CachingKey RegistrationFieldAnswerPattern => new("Nop.nexport.registrationfield.answer.");
}
