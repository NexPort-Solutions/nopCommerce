using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Archway.Extensions;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Plugin.Misc.Nexport.Archway.Services;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories
{
    public class ArchwayStudentEmployeeRegistrationFieldModelFactory : IArchwayStudentEmployeeRegistrationFieldModelFactory
    {
        private readonly IArchwayStudentEmployeeRegistrationFieldService _archwayStudentEmployeeRegistrationFieldService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;

        public ArchwayStudentEmployeeRegistrationFieldModelFactory(
            IArchwayStudentEmployeeRegistrationFieldService archwayStudentEmployeeRegistrationFieldService,
            IStateProvinceService stateProvinceService,
            ILocalizationService localizationService)
        {
            _archwayStudentEmployeeRegistrationFieldService = archwayStudentEmployeeRegistrationFieldService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
        }

        public async Task<ArchwayStudentEmployeeRegistrationFieldModel>
            PrepareArchwayStudentEmployeeRegistrationFieldModelAsync(int fieldId)
        {
            var model = new ArchwayStudentEmployeeRegistrationFieldModel { FieldId = fieldId };

            var storeRecords = await _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var storeAbbreviations = storeRecords.Select(r => r.State).Distinct().ToList();

            var states = (await _stateProvinceService.GetStateProvincesAsync())
                .Where(s => storeAbbreviations.Contains(s.Abbreviation)).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "" });

                foreach (var s in states)
                {
                    var stateName = await _localizationService.GetLocalizedAsync(s, x => x.Name);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = stateName,
                        Value = stateName
                    });
                }
            }

            return model;
        }

        public async Task<ArchwayStudentEmployeeRegistrationFieldOptionModel>
            PrepareArchwayStudentEmployeeRegistrationFieldOptionModelAsync(int fieldId)
        {
            var model = new ArchwayStudentEmployeeRegistrationFieldOptionModel
            {
                FieldId = fieldId
            };

            foreach (var prop in model.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(string))
                {
                    var propAttribute = prop.GetCustomAttribute<ArchwayStudentRegistrationFieldControlAttribute>();
                    if (propAttribute != null)
                    {
                        var keyMapping =
                            await _archwayStudentEmployeeRegistrationFieldService
                                .GetArchwayStudentRegistrationFieldKeyMapping(propAttribute.ControlName);
                        if (!string.IsNullOrWhiteSpace(keyMapping?.FieldKey))
                        {
                            prop.SetValue(model, keyMapping.FieldKey, null);
                        }
                    }
                }
            }

            return model;
        }

        public async Task<IList<ArchwayStoreCityModel>> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem)
        {
            if (string.IsNullOrWhiteSpace(state))
                return new List<ArchwayStoreCityModel>();

            var stateProvince = (await _stateProvinceService.GetStateProvincesAsync()).FirstOrDefault(x => x.Name == state);
            if (stateProvince == null)
                return new List<ArchwayStoreCityModel>();

            var storeRecords = await _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var cities = storeRecords
                .Where(r => r.State == stateProvince.Abbreviation)
                .OrderBy(r => r.City)
                .Select(r => r.City)
                .Distinct()
                .ToList();

            return cities.Select(city => new ArchwayStoreCityModel { name = city }).ToList();
        }

        public async Task<IList<ArchwayStoreAddressModel>> GetArchwayStoreAddressesByCity(string city,
            bool addSelectAddressItem)
        {
            if (string.IsNullOrWhiteSpace(city))
                return new List<ArchwayStoreAddressModel>();

            var storeRecords = await _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var records = storeRecords
                .Where(r => r.City == city)
                .OrderBy(r => r.Address)
                .ToList();

            return records.Select(record =>
                new ArchwayStoreAddressModel { name = record.Address, storeNumber = record.Id, storeType = record.StoreType })
                .ToList();
        }

        public async Task<IList<ArchwayStoreEmployeePositionModel>> GetArchwayStoreEmployeePositionsByStore(
            string storeNumber, bool addSelectPositionItem)
        {
            if (string.IsNullOrWhiteSpace(storeNumber))
                return new List<ArchwayStoreEmployeePositionModel>();

            var storeRecord = await _archwayStudentEmployeeRegistrationFieldService
                .GetArchwayStoreRecordInfo(int.Parse(storeNumber));

            if (storeRecord == null)
                return new List<ArchwayStoreEmployeePositionModel>();

            var employePositions =
                (await _archwayStudentEmployeeRegistrationFieldService
                    .GetArchwayStoreEmployeePositions(storeRecord.StoreType))
                    .OrderBy(p => p.JobTitle);

            return employePositions.Select(position => new ArchwayStoreEmployeePositionModel { name = position.JobTitle }).ToList();
        }
    }
}
