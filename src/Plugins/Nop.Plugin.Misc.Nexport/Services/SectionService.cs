using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ISectionService
{
    Task<GetDescriptionResponse?> GetSectionDescription(Guid sectionId);
    Task<SectionResponse?> GetSectionDetails(Guid sectionId);
    Task<SectionEnrollmentsResponse?> GetSectionEnrollmentDetails(Guid orgId, Guid userId, Guid syllabusId);
    Task<GetObjectivesResponse?> GetSectionObjectives(Guid sectionId);
}

public class SectionService : ISectionService
{
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;

    public SectionService(NexportApiService nexportApi, HelperService helper)
    {
        _nexportApi = nexportApi;
        _helper = helper;
    }

    public Task<SectionResponse?> GetSectionDetails(Guid sectionId)
        => _helper.Do(s => _nexportApi.GetSectionDetails((s.Url, s.Token), sectionId));

    public Task<GetDescriptionResponse?> GetSectionDescription(Guid sectionId)
        => _helper.Do(s => _nexportApi.GetSectionDescription((s.Url, s.Token), sectionId));

    public Task<GetObjectivesResponse?> GetSectionObjectives(Guid sectionId)
        => _helper.Do(s => _nexportApi.GetSectionObjectives((s.Url, s.Token), sectionId));

    public Task<SectionEnrollmentsResponse?> GetSectionEnrollmentDetails(Guid orgId, Guid userId, Guid syllabusId)
        => _helper.Do(s => _nexportApi.GetSectionEnrollment((s.Url, s.Token), orgId, userId, syllabusId));
}
