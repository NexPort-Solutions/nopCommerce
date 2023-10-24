using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Services.Directory;
using Nop.Services.Localization;
using FieldModel = Nop.Plugin.Misc.Nexport.Archway.Models.StudentEmployeeRegistrationFieldModel;
using FieldOptionModel = Nop.Plugin.Misc.Nexport.Archway.Models.StudentEmployeeRegistrationFieldOptionModel;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories;

public interface IStudentEmployeeRegistrationFieldModelFactory
{
    Task<Result<FieldOptionModel, string>> StudentEmployeeRegistrationFieldOptionModel(int fieldId);
    Task<Result<FieldModel, string>> EditStudentEmployeeRegistrationFieldModel(int customerId, int fieldId);
    Task<List<StoreCityModel>> GetStoreCities(string stateName);
    Task<List<StoreAddressModel>> GetStoreAddresses(string city, string stateName);
    Task<List<StoreEmployeePositionModel>> GetStoreEmployeePositions(int storeNumber);
    Task<List<SelectListItem>> GetAvailableAddresses(string city, string state);
    Task<List<SelectListItem>> GetAvailableCities(string state);
    Task<List<SelectListItem>> GetAvailableEmploteePositions(int storeNumber);
    Task<List<SelectListItem>> GetAvailableStates();
}

public class StudentEmployeeRegistrationFieldModelFactory : IStudentEmployeeRegistrationFieldModelFactory
{
    private readonly IStoreRecordInfoService _storeRecordInfoService;
    private readonly IStudentRegistrationFieldKeyMappingService _studentRegistrationFieldKeyMappingService;
    private readonly IStudentRegistrationFieldAnswerService _studentRegistrationFieldAnswerService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreEmployeePositionService _storeEmployeePositionService;

    public StudentEmployeeRegistrationFieldModelFactory(
        IStudentRegistrationFieldKeyMappingService studentRegistrationFieldKeyMappingService,
        IStateProvinceService stateProvinceService,
        ILocalizationService localizationService,
        IStudentRegistrationFieldAnswerService studentRegistrationFieldAnswerService,
        IStoreEmployeePositionService storeEmployeePositionService,
        IStoreRecordInfoService storeRecordInfoService)
    {
        _studentRegistrationFieldKeyMappingService = studentRegistrationFieldKeyMappingService;
        _stateProvinceService = stateProvinceService;
        _localizationService = localizationService;
        _studentRegistrationFieldAnswerService = studentRegistrationFieldAnswerService;
        _storeEmployeePositionService = storeEmployeePositionService;
        _storeRecordInfoService = storeRecordInfoService;
    }

    private static SelectListItem ToSelectListItem(string keyAndValue) => new(keyAndValue, keyAndValue);

    public async Task<List<SelectListItem>> GetAvailableStates()
    {
        var storeStates = (await _storeRecordInfoService.GetAll()).Select(r => r.State).Distinct().ToList();
        return await (await _stateProvinceService.GetStateProvincesAsync())
            .Where(state => storeStates.Contains(state.Abbreviation))
            .SelectAwait(async state => await _localizationService.GetLocalizedAsync(state, state => state.Name))
            .Select(ToSelectListItem)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetAvailableCities(string state)
        => (await GetStoreCities(state)).ConvertAll(city => ToSelectListItem(city.Name));

    public async Task<List<SelectListItem>> GetAvailableAddresses(string city, string state)
        => (await GetStoreAddresses(city, state)).ConvertAll(address => ToSelectListItem(address.Name));

    public async Task<List<SelectListItem>> GetAvailableEmploteePositions(int storeNumber)
        => (await GetStoreEmployeePositions(storeNumber)).ConvertAll(position => ToSelectListItem(position.Name));

    public async Task<Result<FieldOptionModel, string>> StudentEmployeeRegistrationFieldOptionModel(int fieldId)
    {
        if ((await getKey(nameof(FieldModel.StoreLocationState))) is { } storeStateFieldKey
            && (await getKey(nameof(FieldModel.StoreLocationCity))) is { } storeCityFieldKey
            && (await getKey(nameof(FieldModel.StoreLocationAddress))) is { } storeAddressFieldKey
            && (await getKey(nameof(FieldModel.StoreNumber))) is { } storeIdFieldKey
            && (await getKey(nameof(FieldModel.StoreType))) is { } storeTypeFieldKey
            && (await getKey(nameof(FieldModel.EmployeeId))) is { } employeeIdFieldKey
            && (await getKey(nameof(FieldModel.EmployeePosition))) is { } employeePositionFieldKey)
        {
            var model = new FieldOptionModel
            {
                FieldId = fieldId,
                StoreStateFieldKey = storeStateFieldKey,
                StoreCityFieldKey = storeCityFieldKey,
                StoreAddressFieldKey = storeAddressFieldKey,
                StoreIdFieldKey = storeIdFieldKey,
                StoreTypeFieldKey = storeTypeFieldKey,
                EmployeeIdFieldKey = employeeIdFieldKey,
                EmployeePositionFieldKey = employeePositionFieldKey,
            };
            return Okay(model);
        }
        return Error("Missing field key.");
        async Task<string?> getKey(string fieldControlName) => (await _studentRegistrationFieldKeyMappingService.GetByName(fieldControlName))?.FieldKey;
    }

    public async Task<Result<FieldModel, string>> EditStudentEmployeeRegistrationFieldModel(int customerId, int fieldId)
    {
        var currentAnswers = await _studentRegistrationFieldAnswerService.GetAll(customerId, fieldId);
        if (getValue("StoreStateField") is { } state
            && getValue("StoreCityField") is { } city
            && getValue("StoreAddressField") is { } address
            && getValue("EmployeePositionField") is { } employeePosition
            && int.TryParse(getValue("StoreIdField") ?? "0", out var storeNumber)
            && getValue("StoreTypeField") is { } storeType
            && getValue("EmployeeIdField") is { } employeeId)
        {
            var model = new FieldModel
            {
                FieldId = fieldId,
                StoreLocationState = state,
                StoreLocationCity = city,
                StoreLocationAddress = address,
                StoreNumber = storeNumber,
                StoreType = storeType,
                EmployeePosition = employeePosition,
                EmployeeId = employeeId,
            };
            return Okay(model);
        }
        return Error("Missing value.");
        string? getValue(string key) => currentAnswers.Find(answer => answer.FieldKey == key)?.TextValue;
    }

    private async Task<StateProvince?> GetState(string stateName)
        => (await _stateProvinceService.GetStateProvincesAsync()).FirstOrDefault(state => state.Name == stateName);

    public async Task<List<StoreCityModel>> GetStoreCities(string stateName)
    {
        if (await GetState(stateName) is not { Abbreviation: var stateAbbreviation })
        {
            return new List<StoreCityModel>();
        }
        return (await _storeRecordInfoService.GetAll())
            .Where(storeRecord => storeRecord.State == stateAbbreviation)
            .Select(storeRecord => storeRecord.City)
            .Distinct()
            .Select(city => new StoreCityModel { Name = city })
            .ToList();
    }

    public async Task<List<StoreAddressModel>> GetStoreAddresses(string city, string stateName)
    {
        if (await GetState(stateName) is not { Abbreviation: var stateAbbreviation })
        {
            return new List<StoreAddressModel>();
        }
        return (await _storeRecordInfoService.GetAll())
            .Where(storeRecordInfo => storeRecordInfo.City == city && storeRecordInfo.State == stateAbbreviation)
            .Select(record => new StoreAddressModel { Name = record.Address, StoreNumber = record.StoreNumber, StoreType = record.StoreType })
            .ToList();
    }

    public async Task<List<StoreEmployeePositionModel>> GetStoreEmployeePositions(int storeNumber)
    {
        var storeRecord = await _storeRecordInfoService.GetByNumber(storeNumber);
        return (await _storeEmployeePositionService.GetAll(storeRecord.StoreType))
            .ConvertAll(position => new StoreEmployeePositionModel { Name = position.JobTitle, Id = position.Id });
    }
}
