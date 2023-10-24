using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Plugins;
using StackExchange.Profiling.Internal;
using StoreMapping = Nop.Plugin.Misc.Nexport.Domain.RegistrationField.StoreMapping;
using Type = Nop.Plugin.Misc.Nexport.Domain.RegistrationField.RegistrationField.RegistrationFieldType;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IRegistrationFieldService
{
    Task<RegistrationField?> GetById(int fieldId, int? categoryId = null);
    Task<List<RegistrationField>> GetAll(int storeId);
    Task<List<RegistrationField>> GetAllByCategoryId(int categoryId);
    Task<IPagedList<RegistrationField>> GetAllPaginated(IList<int> storeIds, int pageIndex = 0, int pageSize = int.MaxValue);
    Task Insert(RegistrationField registrationField);
    Task Delete(RegistrationField registrationField);
    Task Update(RegistrationField registrationField);
    Task<Option?> GetOptionById(int fieldOptionId, int? fieldId = null);
    Task<List<Option>> GetOptions(int? fieldId = null);
    Task<IPagedList<Option>> GetOptionsPaginated(int fieldId, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<IPagedList<RegistrationField>> GetAllWithAnswersPaginated(int customerId, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<List<RegistrationField>> GetManyWithAnswers(int customerId, int? storeId = null);
    Task InsertOption(Option registrationFieldOption);
    Task DeleteOption(Option registrationFieldOption);
    Task UpdateOption(Option registrationFieldOption);
    Task<Category?> GetCategoryById(int fieldCategoryId);
    Task<List<Category>> GetCategories();
    Task<List<Category>> GetCategories(IList<int> fieldCategoryIds);
    Task<IPagedList<Category>> GetCategoriesPagination(int pageIndex = 0, int pageSize = int.MaxValue);
    Task InsertCategory(Category registrationFieldCategory);
    Task DeleteCategory(Category registrationFieldCategory);
    Task UpdateCategory(Category registrationFieldCategory);
    Task<StoreMapping?> GetStoreMappingById(int fieldStoreMappingId);
    Task<List<StoreMapping>> GetStoreMappings(int fieldId);
    Task InsertStoreMapping(StoreMapping registrationFieldStoreMapping);
    Task DeleteStoreMapping(StoreMapping registrationFieldStoreMapping);
    Task<Answer?> GetAnswerById(int fieldAnswerId);
    Task<List<Answer>> GetAnswers(int customerId, int? fieldId = null);
    Task<IPagedList<Answer>> GetAnswersPaginated(int customerId, int? fieldId = null, int pageIndex = 0, int pageSize = int.MaxValue);
    Task InsertAnswer(Answer answer);
    Task DeleteAnswer(Answer answer);
    Task UpdateAnswer(Answer answer);
    Task InsertSynchronizationQueueItem(SynchronizationQueueItem queueItem);
    Task DeleteSynchronizationQueueItem(SynchronizationQueueItem queueItem);
    Task UpdateSynchronizationQueueItem(SynchronizationQueueItem queueItem);
    Task<Answer?> GetAnswerByFieldOption(int customerId, int fieldId, int fieldOptionId);
    Task<bool> HasCustomRenderForStores(int fieldId, IList<int> storeIds, string? customFieldRender);
    Task SaveMany(Customer customer, Dictionary<int, string> fields);
    Task<List<string>> GetCustomWarnings(Dictionary<int, Dictionary<string, string>> fields);
    Task<List<SelectListItem>> GetCategoryList();
    Task<List<CustomRender>> GetCustomRenders();
    Task<List<string>> GetWarnings(Dictionary<int, string> fields);
    Task<Dictionary<int, Dictionary<string, string>>> Parse(IFormCollection form, int? storeId = null);
    Task<Dictionary<int, string>> ParseMany(IFormCollection form, int? storeId = null);
}

public class RegistrationFieldService : IRegistrationFieldService
{
    private const char FIELD_MULTI_VALUE_SEPERATOR = ',';
    private const string NONE = "None";

    private readonly IRepository<RegistrationField> _registrationFields;
    private readonly IRepository<Option> _options;
    private readonly IRepository<Category> _categorys;
    private readonly IRepository<StoreMapping> _storeMappings;
    private readonly IRepository<Answer> _answers;
    private readonly IRepository<SynchronizationQueueItem> _synchronizationQueueItems;
    private readonly IRepository<Store> _stores;
    private readonly IStoreService _store;
    private readonly IPluginManager<IRegistrationFieldCustomRender> _customRenderPluginManager;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _cacheManager;

    public RegistrationFieldService(
        IRepository<RegistrationField> registrationFields,
        IRepository<Option> options,
        IRepository<Category> categorys,
        IRepository<StoreMapping> storeMappings,
        IRepository<Answer> answers,
        IRepository<SynchronizationQueueItem> synchronizationQueueItems,
        IRepository<Store> stores,
        IStoreService store,
        IPluginManager<IRegistrationFieldCustomRender> customRenderPluginManager,
        IDateTimeHelper dateTimeHelper,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager)
    {
        _registrationFields = registrationFields;
        _options = options;
        _categorys = categorys;
        _storeMappings = storeMappings;
        _answers = answers;
        _synchronizationQueueItems = synchronizationQueueItems;
        _stores = stores;
        _store = store;
        _customRenderPluginManager = customRenderPluginManager;
        _dateTimeHelper = dateTimeHelper;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
    }

    public Task<RegistrationField?> GetById(int fieldId, int? categoryId = null) => categoryId switch
    {
        null => _registrationFields.Table.Where(field => field.Id == fieldId).FirstOrDefaultAsync(),
        _ => _registrationFields.Table.Where(field => field.Id == fieldId).Where(field => field.FieldCategoryId == categoryId).FirstOrDefaultAsync(),
    };

    public async Task<List<RegistrationField>> GetAll(int storeId)
    {
        var fieldStoreMappingsForCurrentStore = await _storeMappings.Table
            .Where(storeMapping => storeMapping.StoreId == storeId)
            .Select(storeMapping => storeMapping.FieldId)
            .ToListAsync();
        var fieldOStoreMappingsForOtherStores = await _storeMappings.Table
            .Where(storeMapping => storeMapping.StoreId != storeId)
            .Select(storeMapping => storeMapping.FieldId)
            .ToListAsync();
        var fields = _registrationFields.Table
            .Where(field => field.IsActive
                && (fieldStoreMappingsForCurrentStore.Contains(field.Id)
                    || !fieldOStoreMappingsForOtherStores.Contains(field.Id)));
        return await fields.ToListAsync();
    }

    public Task<List<RegistrationField>> GetAllByCategoryId(int categoryId)
        => _registrationFields.Table.Where(field => field.FieldCategoryId == categoryId).ToListAsync();

    public async Task<IPagedList<RegistrationField>> GetAllPaginated(IList<int> storeIds, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _registrationFields.Table;
        // check if contains 0 because nopselect puts 0 in list of ids if "all" is selected
        if (storeIds?.Contains(0) is not false)
        {
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        var storeQuery = _stores.Table
            .Where(store => storeIds.Contains(store.Id))
            .Select(store => store.Id)
            .ToList();
        var storeMappingQuery = _storeMappings.Table
            .Where(fieldStoreMapping => storeQuery.Contains(fieldStoreMapping.StoreId))
            .Select(fieldStoreMapping => fieldStoreMapping.FieldId)
            .ToList();

        query = query.Where(field => storeMappingQuery.Contains(field.Id));
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public Task<IPagedList<RegistrationField>> GetAllWithAnswersPaginated(int customerId, int? storeId = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var registrationFieldWithAnswerIds = _answers.Table
            .Where(fieldAnswer => fieldAnswer.CustomerId == customerId)
            .GroupBy(fieldAnswer => fieldAnswer.FieldId)
            .Select(fieldAnswer => fieldAnswer.FirstOrDefault())
            .WhereNotNull()
            .Select(fieldAnswer => fieldAnswer.FieldId);
        if (storeId is not null)
        {
            registrationFieldWithAnswerIds = registrationFieldWithAnswerIds
                .Where(id => _storeMappings.Table
                    .Where(mapping => mapping.StoreId == storeId)
                    .Select(mapping => mapping.FieldId)
                    .Contains(id)
                    || !_storeMappings.Table
                        .Where(mapping => mapping.FieldId == id)
                        .Select(mapping => mapping.FieldId)
                        .Contains(id));
        }
        return _registrationFields.Table
            .Where(field => registrationFieldWithAnswerIds.Contains(field.Id))
            .ToPagedListAsync(pageIndex, pageSize);
    }

    public Task<List<RegistrationField>> GetManyWithAnswers(int customerId, int? storeId = null)
    {
        var registrationFieldWithAnswerIds = _answers.Table
            .Where(fieldAnswer => fieldAnswer.CustomerId == customerId)
            .GroupBy(fieldAnswer => fieldAnswer.FieldId)
            .Select(fieldAnswers => fieldAnswers.FirstOrDefault())
            .WhereNotNull()
            .Select(fieldAnswer => fieldAnswer.FieldId);
        if (storeId is not null)
        {
            var registrationFieldIdsByStore = _storeMappings.Table
                .Where(mapping => mapping.StoreId == storeId)
                .Select(mapping => mapping.FieldId);
            registrationFieldWithAnswerIds = registrationFieldWithAnswerIds.Intersect(registrationFieldIdsByStore).ToList();
        }
        var query = _registrationFields.Table.Where(field => registrationFieldWithAnswerIds.Contains(field.Id));
        return query.ToListAsync();
    }

    public Task Insert(RegistrationField registrationField) => _registrationFields.InsertAsync(registrationField);
    public Task Delete(RegistrationField registrationField) => _registrationFields.DeleteAsync(registrationField);
    public Task Update(RegistrationField registrationField) => _registrationFields.UpdateAsync(registrationField);

    public Task<Option?> GetOptionById(int fieldOptionId, int? fieldId = null) => (fieldId switch
    {
        null => _options.GetByIdAsync(fieldOptionId), // SingleOrDefaultAsync and GetByIdAsync can return null.
        _ => _options.Table.SingleOrDefaultAsync(fieldOption => fieldOption.Id == fieldOptionId && fieldOption.FieldId == fieldId),
    })!;

    public Task<List<Option>> GetOptions(int? fieldId = null) => fieldId switch
    {
        null => _options.Table.ToListAsync(),
        _ => _options.Table.Where(fieldOption => fieldOption.FieldId == fieldId).ToListAsync(),
    };

    public Task<IPagedList<Option>> GetOptionsPaginated(int fieldId, int pageIndex = 0, int pageSize = int.MaxValue)
        => _cacheManager.GetAsync(
            CacheKey.RegistrationFieldOptionAll,
            () => _options.Table.Where(fieldOption => fieldOption.FieldId == fieldId).ToPagedListAsync(pageIndex, pageSize));

    public Task InsertOption(Option registrationFieldOption) => _options.InsertAsync(registrationFieldOption);
    public Task DeleteOption(Option registrationFieldOption) => _options.DeleteAsync(registrationFieldOption);
    public Task UpdateOption(Option registrationFieldOption) => _options.UpdateAsync(registrationFieldOption);
    public Task<Category?> GetCategoryById(int fieldCategoryId) => _categorys.GetByIdAsync(fieldCategoryId)!; // GetByIdAsync can return null.

    public Task<List<Category>> GetCategories()
        => _categorys.Table
            .OrderBy(store => store.DisplayOrder)
            .ThenBy(store => store.Id)
            .ToListAsync();

    public async Task<List<Category>> GetCategories(IList<int> fieldCategoryIds)
        => await fieldCategoryIds.Select(GetCategoryById).SelectAwait(async task => await task).WhereNotNull().ToListAsync();

    public Task<IPagedList<Category>> GetCategoriesPagination(int pageIndex = 0, int pageSize = int.MaxValue)
        => _cacheManager.GetAsync(
            CacheKey.RegistrationFieldCategoryAll,
            () => _categorys.Table.ToPagedListAsync(pageIndex, pageSize));

    public Task InsertCategory(Category registrationFieldCategory) => _categorys.InsertAsync(registrationFieldCategory);
    public Task DeleteCategory(Category registrationFieldCategory) => _categorys.DeleteAsync(registrationFieldCategory);
    public Task UpdateCategory(Category registrationFieldCategory) => _categorys.UpdateAsync(registrationFieldCategory);
    public Task<StoreMapping?> GetStoreMappingById(int fieldStoreMappingId) => _storeMappings.GetByIdAsync(fieldStoreMappingId)!; // GetByIdAsync can return null.
    public Task<List<StoreMapping>> GetStoreMappings(int fieldId) => _storeMappings.Table.Where(storeMapping => storeMapping.FieldId == fieldId).ToListAsync();

    public async Task InsertStoreMapping(StoreMapping registrationFieldStoreMapping)
    {
        if (!await _storeMappings.Table.AnyAsync(existing =>
            existing.FieldId == registrationFieldStoreMapping.FieldId && existing.StoreId == registrationFieldStoreMapping.StoreId))
        {
            await _storeMappings.InsertAsync(registrationFieldStoreMapping);
        }
    }

    public Task DeleteStoreMapping(StoreMapping registrationFieldStoreMapping) => _storeMappings.DeleteAsync(registrationFieldStoreMapping);
    public Task<Answer?> GetAnswerById(int fieldAnswerId) => _answers.GetByIdAsync(fieldAnswerId)!; // GetByIdAsync can return null.

    public Task<List<Answer>> GetAnswers(int customerId, int? fieldId = null) => fieldId switch
    {
        null => _answers.Table.Where(answer => answer.CustomerId == customerId).ToListAsync(),
        _ => _answers.Table.Where(answer => answer.CustomerId == customerId && answer.FieldId == fieldId).ToListAsync(),
    };

    public Task<IPagedList<Answer>> GetAnswersPaginated(int customerId, int? fieldId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        => _cacheManager.GetAsync(
            CacheKey.RegistrationFieldAnswerAll,
            () => fieldId switch
            {
                null => _answers.Table.Where(answer => answer.CustomerId == customerId).ToPagedListAsync(pageIndex, pageSize),
                _ => _answers.Table.Where(answer => answer.CustomerId == customerId && answer.FieldId == fieldId).ToPagedListAsync(pageIndex, pageSize),
            });

    public Task<Answer?> GetAnswerByFieldOption(int customerId, int fieldId, int fieldOptionId)
        => _answers.Table.FirstOrDefaultAsync(answer => answer.CustomerId == customerId && answer.FieldId == fieldId && answer.FieldOptionId == fieldOptionId)!;

    public Task InsertAnswer(Answer answer)
    {
        if ((answer.FieldOptionId is null && !_answers.Table.Any(matchesCustomerAndField))
            || !_answers.Table.Any(matchesCustomerFieldAndFieldOption))
        {
            return _answers.InsertAsync(answer);
        }
        return Task.CompletedTask;

        bool matchesCustomerAndField(Answer fieldAnswer)
            => fieldAnswer.CustomerId == answer.CustomerId && fieldAnswer.FieldId == answer.FieldId;
        bool matchesCustomerFieldAndFieldOption(Answer fieldAnswer)
            => matchesCustomerAndField(fieldAnswer) && fieldAnswer.FieldOptionId == answer.FieldOptionId;
    }

    public Task DeleteAnswer(Answer answer) => _answers.DeleteAsync(answer);
    public Task UpdateAnswer(Answer answer) => _answers.UpdateAsync(answer);
    public Task InsertSynchronizationQueueItem(SynchronizationQueueItem queueItem) => _synchronizationQueueItems.InsertAsync(queueItem);
    public Task DeleteSynchronizationQueueItem(SynchronizationQueueItem queueItem) => _synchronizationQueueItems.DeleteAsync(queueItem);
    public Task UpdateSynchronizationQueueItem(SynchronizationQueueItem queueItem) => _synchronizationQueueItems.UpdateAsync(queueItem);

    public async Task<bool> HasCustomRenderForStores(int fieldId, IList<int> storeIds, string? customFieldRender)
    {
        var stores = (await _store.GetAllStoresAsync()).Select(store => store.Id);
        var fieldIds = _registrationFields.Table.Where(field => field.CustomFieldRender == customFieldRender).Select(field => field.Id);
        return storeIds.Count switch
        {
            not 0 => await fieldIds.Where(id => id != fieldId).AnyAwaitAsync(async id => await match(id, storeIds)),
            _ => await fieldIds.AnyAwaitAsync(async id => await match(id, stores))
        };

        Task<bool> match(int id, IEnumerable<int> storesToSearch)
            => storesToSearch.AnyAwaitAsync(async storeId => await _storeMappings.Table.AnyAsync(field => field.FieldId == id && field.StoreId == storeId));
    }

    public async Task<Dictionary<int, Dictionary<string, string>>> Parse(IFormCollection form, int? storeId = null)
    {
        var result = new Dictionary<int, Dictionary<string, string>>();
        var store = await _storeContext.GetCurrentStoreAsync();
        foreach (var field in await GetAll(storeId ?? store.Id))
        {
            await ParseField(form, result, field);
        }
        return result.Where(item => item.Value is not null).ToDictionary(x => x.Key, x => x.Value!);
    }

    private async Task ParseField(IFormCollection form, Dictionary<int, Dictionary<string, string>> result, RegistrationField field)
    {
        if (field is not { Type: Type.CustomType, IsActive: true, Id: var id, CustomFieldRender: var render }
            || await _customRenderPluginManager.LoadPluginBySystemNameAsync(render) is not { } customRender)
        {
            return;
        }
        var parseResult = await customRender.ParseCustomRegistrationFields(id, form);
        result.Add(id, parseResult);
    }

    public virtual async Task<List<string>> GetWarnings(Dictionary<int, string> fields)
    {
        var warnings = new List<string>();
        foreach (var field in fields)
        {
            await GetWarning(warnings, field);
        }
        return warnings;
    }

    private async Task GetWarning(List<string> warnings, KeyValuePair<int, string> field)
    {
        if (await GetById(field.Key) is { } registrationField
            && registrationField?.Name?.TruncateAtWord(5) is { } fieldNameTruncated)
        {
            WarnIfInvalidValue(warnings, field, registrationField, fieldNameTruncated);
            WarnIfMissingRequiredValue(warnings, field, registrationField, fieldNameTruncated);
        }
    }

    private static void WarnIfMissingRequiredValue(List<string> warnings, in KeyValuePair<int, string> field, RegistrationField? registrationField, string? fieldNameTruncated)
    {
        if (registrationField?.IsRequired is true && string.IsNullOrWhiteSpace(field.Value))
        {
            warnings.Add($"Field {fieldNameTruncated} value is empty");
        }
    }

    private static void WarnIfInvalidValue(List<string> warnings, in KeyValuePair<int, string> field, RegistrationField? registrationField, string fieldNameTruncated)
    {
        if (!string.IsNullOrWhiteSpace(field.Value) && getWarning(registrationField?.Type, field.Value, fieldNameTruncated) is { } message)
        {
            warnings.Add(message);
        }

        static string? getWarning(Type? type, string value, string fieldNameTruncated) => type switch
        {
            Type.Email when !value.IsValidEmail() => $"Field {fieldNameTruncated} value is not a valid email.",
            Type.DateOnly when !value.IsValidDateFormat("MM/dd/yyyy") => $"Field {fieldNameTruncated} value is not a valid date format.",
            Type.DateTime when !value.IsValidDateFormat("MM/dd/yyyy HH:mm") => $"Field {fieldNameTruncated} value is not a valid date time format.",
            _ => null
        };
    }

    public virtual Task<List<string>> GetCustomWarnings(Dictionary<int, Dictionary<string, string>> fields) => Task.FromResult(new List<string>());

    public virtual async Task SaveMany(Customer customer, Dictionary<int, string> fields)
    {
        await foreach (var (value, field) in GetAll(fields))
        {
            if (field.Type is not Type.SelectCheckbox)
            {
                await InsertAnswer(ToFieldAnswer(customer, value, field));
                continue;
            }
            foreach (var optionId in value.Split(FIELD_MULTI_VALUE_SEPERATOR))
            {
                var answer = new Answer
                {
                    CustomerId = customer.Id,
                    FieldId = field.Id,
                    UtcDateCreated = DateTime.UtcNow,
                    UtcDateModified = DateTime.UtcNow,
                    FieldOptionId = int.TryParse(optionId, out var selectionResult) ? selectionResult : null,
                    IsCustomField = false,
                };
                await InsertAnswer(answer);
            }
        }
    }

    public async IAsyncEnumerable<(string, RegistrationField)> GetAll(Dictionary<int, string> fields)
    {
        foreach (var field in fields)
        {
            if (await GetById(field.Key) is { } registrationField)
            {
                yield return (field.Value, registrationField);
            }
        }
    }

    public Answer ToFieldAnswer(Customer customer, string value, RegistrationField field)
        => new()
        {
            CustomerId = customer.Id,
            FieldId = field.Id,
            UtcDateCreated = DateTime.UtcNow,
            UtcDateModified = DateTime.UtcNow,
            FieldOptionId = field.Type is Type.SelectDropDown && int.TryParse(value, out var selectionResult) ? selectionResult : null,
            IsCustomField = field.Type is Type.CustomType,
            BooleanValue = field.Type is Type.Boolean && bool.TryParse(value, out var booleanResult) ? booleanResult : null,
            DateTimeValue = field.Type is Type.DateOnly or Type.DateTime && DateTime.TryParse(value, out var dateTimeResult) ? _dateTimeHelper.ConvertToUtcTime(dateTimeResult) : null,
            TextValue = field.Type is Type.Text or Type.Email ? value : null,
            NumericValue = field.Type is Type.Numeric && int.TryParse(value, out var intResult) ? intResult : null,
        };

    public async Task<List<SelectListItem>> GetCategoryList()
    {
        var registrationFieldCategories = await GetCategories();
        var result = registrationFieldCategories.ConvertAll(category => new SelectListItem(category.Title, category.Id.ToString(CultureInfo.InvariantCulture)));
        result.Insert(0, new SelectListItem(NONE, string.Empty));
        return result;
    }

    public async Task<Dictionary<int, string>> ParseMany(IFormCollection form, int? storeId = null)
    {
        storeId ??= (await _storeContext.GetCurrentStoreAsync()).Id;
        var result = new Dictionary<int, string>();
        var registrationFields = (await GetAll(storeId.Value))
            .Where(registrationField => registrationField.Type is not Type.CustomType);
        foreach (var field in registrationFields)
        {
            if (!field.IsActive)
            {
                continue;
            }
            var controlValue = form[$"{Defaults.RegistrationFieldPrefix}-{field.Id}"];
            if (!StringValues.IsNullOrEmpty(controlValue))
            {
                result.Add(field.Id, controlValue.ToString().Trim());
            }
        }
        return result;
    }

    public async Task<List<CustomRender>> GetCustomRenders()
        => await (await _customRenderPluginManager.LoadAllPluginsAsync())
            .Select(plugin => plugin.PluginDescriptor)
            .Where(descriptor => descriptor.PluginType.GetCustomAttributes(typeof(CustomRegistrationFieldRenderAttribute), false).Length > 0)
            .Select(descriptor => new CustomRender
            {
                Name = descriptor.FriendlyName,
                RenderAssembly = descriptor.SystemName,
            })
            .ToListAsync();
}
