using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ITrainingPlanService
{
    Task<GetDescriptionResponse?> GetTrainingPlanDescription(Guid trainingPlanId);
    Task<TrainingPlanResponse?> GetTrainingPlanDetails(Guid trainingPlanId);
    Task<TrainingPlanEnrollmentsResponse?> GetTrainingPlanEnrollmentDetails(Guid orgId, Guid userId, Guid trainingPlanId);
}

public class TrainingPlanService : ITrainingPlanService
{
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;

    public TrainingPlanService(NexportApiService nexportApi, HelperService helper)
    {
        _nexportApi = nexportApi;
        _helper = helper;
    }

    public Task<TrainingPlanResponse?> GetTrainingPlanDetails(Guid trainingPlanId)
        => _helper.Do(s => _nexportApi.GetTrainingPlanDetails((s.Url, s.Token), trainingPlanId));

    public Task<GetDescriptionResponse?> GetTrainingPlanDescription(Guid trainingPlanId)
        => _helper.Do(s => _nexportApi.GetTrainingPlanDescription((s.Url, s.Token), trainingPlanId));

    public Task<TrainingPlanEnrollmentsResponse?> GetTrainingPlanEnrollmentDetails(Guid orgId, Guid userId, Guid trainingPlanId)
        => _helper.Do(s => _nexportApi.GetTrainingPlanEnrollment((s.Url, s.Token), orgId, userId, trainingPlanId));
}
