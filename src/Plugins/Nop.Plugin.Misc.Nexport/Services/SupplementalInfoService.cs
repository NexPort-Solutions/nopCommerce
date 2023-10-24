using System.Globalization;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ISupplementalInfoService
{
    Task<List<QuestionMapping>> GetSupplementalInfoQuestionMappings(int productMappingId);
    Task DeleteRequiredSupplementalInfo(RequiredSupplementalInfo requirement);
    Task DeleteSupplementalInfoAnswer(Answer answer);
    Task DeleteSupplementalInfoAnswerMembership(AnswerMembership answerMembership);
    Task DeleteSupplementalInfoAnswerProcessingQueueItem(AnswerProcessingQueueItem queueItem);
    Task DeleteSupplementalInfoOption(Option option);
    Task DeleteSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation);
    Task DeleteSupplementalInfoQuestion(Question question);
    Task DeleteSupplementalInfoQuestionMapping(QuestionMapping questionMapping);
    Task DeleteSupplementalInfoQuestions(IList<Question> questions);
    Task<IPagedList<Question>> GetAllSupplementalInfoQuestionsPagination(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
    Task<List<Question>> GetAllSupplementalInfoQuestions();
    Task<Question?> GetSupplementalInfoQuestionById(int questionId);
    Task<List<Question>> GetSupplementalInfoQuestionsByIds(int[] questionIds);
    Task InsertSupplementalInfoQuestion(Question question);
    Task UpdateSupplementalInfoQuestion(Question question);
    Task<Option?> GetSupplementalInfoOptionById(int optionId);
    Task<List<Option>> GetSupplementalInfoOptionsByQuestionId(int questionId, bool showHidden = false);
    Task InsertSupplementalInfoOption(Option option);
    Task UpdateSupplementalInfoOption(Option option);
    Task<QuestionMapping?> GetSupplementalInfoQuestionMappingById(int questionMappingId);
    Task<QuestionMapping?> GetSupplementalInfoQuestionMapping(int productMappingId, int questionId);
    Task InsertSupplementalInfoQuestionMapping(QuestionMapping questionMapping);
    Task UpdateSupplementalInfoQuestionMapping(QuestionMapping questionMapping);
    Task<IPagedList<OptionGroupAssociation>> GetSupplementalInfoOptionGroupAssociationsPagination(int optionId, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<List<OptionGroupAssociation>> GetSupplementalInfoOptionGroupAssociations(int optionId, bool excludeInactive = false);
    Task<OptionGroupAssociation?> GetSupplementalInfoOptionGroupAssociationById(int groupAssociationId);
    Task InsertSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation);
    Task UpdateSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation);
    Task InsertSupplementalInfoAnswer(Answer answer);
    Task UpdateSupplementalInfoAnswer(Answer answer);
    Task<List<Answer>> GetSupplementalInfoAnswers(int customerId, int storeId, int? questionId = null);
    Task<IPagedList<Answer>> GetSupplementalInfoAnswersPagination(int customerId, int? questionId = null, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<Answer?> GetSupplementalInfoAnswerById(int answerId);
    Task<IPagedList<Question>> GetSupplementalInfoAnsweredQuestionsPagination(int customerId, int pageIndex = 0, int pageSize = int.MaxValue);
    Task InsertSupplementalInfoAnswerMembership(AnswerMembership answerMembership);
    Task InsertRequiredSupplementalInfo(RequiredSupplementalInfo requirement);
    Task<List<RequiredSupplementalInfo>> GetRequiredSupplementalInfos(int customerId, int storeId, int? questionId = null);
    Task<RequiredSupplementalInfo?> GetRequiredSupplementalInfoByNopProductId(int customerId, int storeId, int questionId);
    Task<bool> HasRequiredSupplementalInfo(int customerId, int storeId);
    Task InsertSupplementalInfoAnswerProcessingQueueItem(AnswerProcessingQueueItem queueItem);
    Task<List<AnswerMembership>> GetSupplementalInfoAnswerMembershipsByAnswerId(int answerId);
    Task<AnswerMembership> GetSupplementalInfoAnswerMembership(Guid membershipId);
    Task<List<SelectListItem>> GetSupplementalInfoQuestionList();
    Task<IEnumerable<int>> GetUnansweredQuestions(int customerId, int storeId, IEnumerable<int> questionIds);
}

public class SupplementalInfoService : ISupplementalInfoService
{
    private readonly IStaticCacheManager _cacheManager;
    private readonly IRepository<Question> _questions;
    private readonly IRepository<Option> _options;
    private readonly IRepository<QuestionMapping> _questionMappings;
    private readonly IRepository<Answer> _answers;
    private readonly IRepository<OptionGroupAssociation> _optionGroupAssociations;
    private readonly IRepository<AnswerMembership> _answerMemberships;
    private readonly IRepository<RequiredSupplementalInfo> _required;
    private readonly IRepository<AnswerProcessingQueueItem> _answerProcessingQueues;
    private readonly IStoreContext _storeContext;

    public SupplementalInfoService(
        IStaticCacheManager cacheManager,
        IRepository<Question> questions,
        IRepository<Option> options,
        IRepository<QuestionMapping> questionMappings,
        IRepository<Answer> answers,
        IRepository<OptionGroupAssociation> optionGroupAssociations,
        IRepository<AnswerMembership> answerMemberships,
        IRepository<RequiredSupplementalInfo> required,
        IRepository<AnswerProcessingQueueItem> answerProcessingQueues,
        IStoreContext storeContext)
    {
        _cacheManager = cacheManager;
        _questions = questions;
        _options = options;
        _questionMappings = questionMappings;
        _answers = answers;
        _optionGroupAssociations = optionGroupAssociations;
        _answerMemberships = answerMemberships;
        _required = required;
        _answerProcessingQueues = answerProcessingQueues;
        _storeContext = storeContext;
    }

    public Task<List<QuestionMapping>> GetSupplementalInfoQuestionMappings(int productMappingId)
        => _questionMappings.Table
            .Where(mapping => mapping.ProductMappingId == productMappingId)
            .ToListAsync();

    public Task<IPagedList<Question>> GetAllSupplementalInfoQuestionsPagination(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        => _cacheManager.GetAsync(
            _cacheManager.PrepareKeyForDefaultCache(CacheKey.SupplementalInfoQuestionAll, pageIndex, pageSize),
            () => _questions.Table.Select(question => question).ToPagedListAsync(pageIndex, pageSize));

    public async Task<List<Question>> GetAllSupplementalInfoQuestions() => await (await GetAllSupplementalInfoQuestionsPagination()).ToListAsync();

    public Task<Question?> GetSupplementalInfoQuestionById(int questionId)
        => _questions.GetByIdAsync(questionId)!;

    public async Task<List<Question>> GetSupplementalInfoQuestionsByIds(int[] questionIds)
    {
        var questions = await _questions.Table
            .Where(question => questionIds.Contains(question.Id))
            .ToListAsync();
        return await questionIds
            .Select(id => questions.Find(question => question.Id == id))
            .WhereNotNull()
            .ToListAsync();
    }

    public Task InsertSupplementalInfoQuestion(Question question) => _questions.InsertAsync(question);
    public Task DeleteSupplementalInfoQuestion(Question question) => _questions.DeleteAsync(question);

    public async Task DeleteSupplementalInfoQuestions(IList<Question> questions)
    {
        foreach (var question in questions)
        {
            await DeleteSupplementalInfoQuestion(question);
        }
    }

    public async Task UpdateSupplementalInfoQuestion(Question question)
    {
        question.UtcDateModified = DateTime.UtcNow;
        await _questions.UpdateAsync(question);
    }

    public Task<Option?> GetSupplementalInfoOptionById(int optionId) => _options.GetByIdAsync(optionId)!; // GetByIdAsync can return null.

    public Task<List<Option>> GetSupplementalInfoOptionsByQuestionId(int questionId, bool showHidden = false)
    {
        var query = _options.Table
            .Where(option => option.QuestionId == questionId);
        return showHidden
            ? query.ToListAsync()
            : query.Where(option => !option.Deleted).ToListAsync();
    }

    public Task InsertSupplementalInfoOption(Option option) => _options.InsertAsync(option);

    public async Task DeleteSupplementalInfoOption(Option option)
    {
        option.Deleted = true;
        await UpdateSupplementalInfoOption(option);
    }

    public async Task UpdateSupplementalInfoOption(Option option)
    {
        option.UtcDateModified = DateTime.UtcNow;
        await _options.UpdateAsync(option);
    }

    public Task<QuestionMapping?> GetSupplementalInfoQuestionMappingById(int questionMappingId)
        => _questionMappings.GetByIdAsync(questionMappingId)!; // GetByIdAsync can return null

    public Task<QuestionMapping?> GetSupplementalInfoQuestionMapping(int productMappingId, int questionId)
        => _questionMappings.Table.FirstOrDefaultAsync(question => question.ProductMappingId == productMappingId && question.QuestionId == questionId)!;

    public async Task InsertSupplementalInfoQuestionMapping(QuestionMapping questionMapping)
    {
        if (await _questionMappings.Table.AnyAsync(questionMapping =>
            questionMapping.QuestionId == questionMapping.QuestionId
                && questionMapping.ProductMappingId == questionMapping.ProductMappingId))
        {
            return;
        }
        await _questionMappings.InsertAsync(questionMapping);
    }

    public Task DeleteSupplementalInfoQuestionMapping(QuestionMapping questionMapping)
        => _questionMappings.DeleteAsync(questionMapping);

    public Task UpdateSupplementalInfoQuestionMapping(QuestionMapping questionMapping)
        => _questionMappings.UpdateAsync(questionMapping);

    public async Task<IPagedList<OptionGroupAssociation>> GetSupplementalInfoOptionGroupAssociationsPagination(int optionId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheKey.SupplementalInfoOptionGroupAssociationsAll, (await _storeContext.GetCurrentStoreAsync()).Id);
        return await _cacheManager.GetAsync(cacheKey, () => groups(optionId, pageIndex, pageSize));

        Task<IPagedList<OptionGroupAssociation>> groups(int optionId, int pageIndex, int pageSize)
            => _optionGroupAssociations.Table
                .Where(groupAssociation => groupAssociation.OptionId == optionId)
                .ToPagedListAsync(pageIndex, pageSize);
    }

    public Task<List<OptionGroupAssociation>> GetSupplementalInfoOptionGroupAssociations(int optionId, bool excludeInactive = false)
    {
        var query = _optionGroupAssociations.Table
            .Where(association => association.OptionId == optionId);
        if (excludeInactive)
        {
            query = query.Where(association => association.IsActive);
        }
        return query.ToListAsync();
    }

    public Task<OptionGroupAssociation?> GetSupplementalInfoOptionGroupAssociationById(int groupAssociationId)
        => _optionGroupAssociations.GetByIdAsync(groupAssociationId)!;

    public async Task InsertSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation)
    {
        if (await _optionGroupAssociations.Table.AnyAsync(groupAssociation =>
            groupAssociation.OptionId == groupAssociation.OptionId
                && groupAssociation.GroupId == groupAssociation.GroupId))
        {
            return;
        }
        await _optionGroupAssociations.InsertAsync(groupAssociation);
    }

    public Task DeleteSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation)
        => _optionGroupAssociations.DeleteAsync(groupAssociation);

    public Task UpdateSupplementalInfoOptionGroupAssociation(OptionGroupAssociation groupAssociation)
        => _optionGroupAssociations.UpdateAsync(groupAssociation);

    public async Task InsertSupplementalInfoAnswer(Answer answer)
    {
        if (!await _answers.Table.AnyAsync(answer =>
            answer.CustomerId == answer.CustomerId
                && answer.OptionId == answer.OptionId
                && answer.QuestionId == answer.QuestionId
                && answer.StoreId == answer.StoreId))
        {
            await _answers.InsertAsync(answer);
        }
    }

    public Task DeleteSupplementalInfoAnswer(Answer answer) => _answers.DeleteAsync(answer);
    public Task UpdateSupplementalInfoAnswer(Answer answer) => _answers.UpdateAsync(answer);

    public Task<List<Answer>> GetSupplementalInfoAnswers(int customerId, int storeId, int? questionId = null)
    {
        var query = _answers.Table
            .Where(answer => answer.CustomerId == customerId && answer.StoreId == storeId);
        if (questionId is not null)
        {
            query = query.Where(answer => answer.QuestionId == questionId);
        }
        return query.ToListAsync();
    }

    public Task<IPagedList<Answer>> GetSupplementalInfoAnswersPagination(
        int customerId,
        int? questionId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        return _cacheManager.GetAsync(
            CacheKey.SupplementalInfoAnswerAll,
            () =>
            {
                var query = _answers
                    .Table
                    .Where(answer => answer.CustomerId == customerId);
                if (questionId is not null)
                {
                    query = query.Where(answer => answer.QuestionId == questionId);
                }
                return query.ToPagedListAsync(pageIndex, pageSize);
            });
    }

    public Task<Answer?> GetSupplementalInfoAnswerById(int answerId) => _answers.GetByIdAsync(answerId)!;

    public Task<IPagedList<Question>> GetSupplementalInfoAnsweredQuestionsPagination(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        => _questions.Table
            .Where(question => _answers.Table
                .Where(answer => answer.CustomerId == customerId)
                .Select(answer => answer.QuestionId)
                .Contains(question.Id))
            .ToPagedListAsync(pageIndex, pageSize);

    public async Task InsertSupplementalInfoAnswerMembership(AnswerMembership answerMembership)
    {
        if (await _answerMemberships.Table.AnyAsync(answerMapping =>
            answerMapping.AnswerId == answerMembership.AnswerId
                && answerMapping.MembershipId == answerMembership.MembershipId))
        {
            return;
        }
        await _answerMemberships.InsertAsync(answerMembership);
    }

    public Task DeleteSupplementalInfoAnswerMembership(AnswerMembership answerMembership)
        => _answerMemberships.DeleteAsync(answerMembership);

    public Task<List<AnswerMembership>> GetSupplementalInfoAnswerMembershipsByAnswerId(int answerId)
        => _answerMemberships.Table
            .Where(answerMembership => answerMembership.AnswerId == answerId)
            .ToListAsync();

    public Task<AnswerMembership> GetSupplementalInfoAnswerMembership(Guid membershipId)
        => _answerMemberships.Table
            .FirstOrDefaultAsync(answerMembership => answerMembership.MembershipId == membershipId);

    public async Task InsertRequiredSupplementalInfo(RequiredSupplementalInfo requirement)
    {
        if (!await _required.Table.AnyAsync(info =>
            info.CustomerId == requirement.CustomerId
                && info.StoreId == requirement.StoreId
                && info.QuestionId == requirement.QuestionId))
        {
            await _required.InsertAsync(requirement);
        }
    }

    public Task DeleteRequiredSupplementalInfo(RequiredSupplementalInfo requirement) => _required.DeleteAsync(requirement);

    public Task<List<RequiredSupplementalInfo>> GetRequiredSupplementalInfos(int customerId, int storeId, int? questionId = null)
    {
        var query = _required.Table
            .Where(info => info.CustomerId == customerId && info.StoreId == storeId);
        if (questionId is not null)
        {
            query = query.Where(info => info.QuestionId == questionId);
        }
        return query.ToListAsync();
    }

    public Task<RequiredSupplementalInfo?> GetRequiredSupplementalInfoByNopProductId(int customerId, int storeId, int questionId)
        => _required.Table
            .FirstOrDefaultAsync(info => info.CustomerId == customerId && info.StoreId == storeId && info.QuestionId == questionId)!; // FirstOrDefaultAsync can return null.

    public Task<bool> HasRequiredSupplementalInfo(int customerId, int storeId)
        => _required.Table
            .AnyAsync(info => info.CustomerId == customerId && info.StoreId == storeId);

    public Task InsertSupplementalInfoAnswerProcessingQueueItem(AnswerProcessingQueueItem queueItem)
        => _answerProcessingQueues.InsertAsync(queueItem);

    public Task DeleteSupplementalInfoAnswerProcessingQueueItem(AnswerProcessingQueueItem queueItem)
        => _answerProcessingQueues.DeleteAsync(queueItem);

    public async Task<List<SelectListItem>> GetSupplementalInfoQuestionList()
        => await (await GetAllSupplementalInfoQuestions())
            .Select(question => new SelectListItem(question.QuestionText, question.Id.ToString(CultureInfo.InvariantCulture)))
            .ToListAsync();

    public async Task<IEnumerable<int>> GetUnansweredQuestions(int customerId, int storeId, IEnumerable<int> questionIds)
    {
        var questionWithAnswerIds = (await GetSupplementalInfoAnswers(customerId, storeId))
            .Where(answer => questionIds.Contains(answer.QuestionId))
            .Select(answer => answer.QuestionId);
        return questionIds.Except(questionWithAnswerIds);
    }
}
