using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public ArchwayStudentEmployeeRegistrationFieldModel PrepareArchwayStudentEmployeeRegistrationFieldModel(
            int fieldId)
        {
            var model = new ArchwayStudentEmployeeRegistrationFieldModel { FieldId = fieldId };

            var storeRecords = _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var storeAbbreviations = storeRecords.Select(r => r.State).Distinct().ToList();

            var states = _stateProvinceService.GetStateProvinces()
                .Where(s => storeAbbreviations.Contains(s.Abbreviation)).ToList();
            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectState"), Value = "" });

                foreach (var s in states)
                {
                    var stateName = _localizationService.GetLocalized(s, x => x.Name);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = stateName,
                        Value = stateName
                    });
                }
            }

            return model;
        }

        public ArchwayStudentEmployeeRegistrationFieldOptionModel PrepareArchwayStudentEmployeeRegistrationFieldOptionModel(int fieldId)
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
                            _archwayStudentEmployeeRegistrationFieldService
                                .GetArchwayStudentRegistrationFieldKeyMapping(fieldId, propAttribute.ControlName);
                        if (!string.IsNullOrWhiteSpace(keyMapping?.FieldKey))
                        {
                            prop.SetValue(model, keyMapping.FieldKey, null);
                        }
                    }
                }
            }

            return model;
        }

        public IList<ArchwayStoreCityModel> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem)
        {
            if (string.IsNullOrWhiteSpace(state))
                return new List<ArchwayStoreCityModel>();

            var stateProvince = _stateProvinceService.GetStateProvinces().FirstOrDefault(x => x.Name == state);
            if (stateProvince == null)
                return new List<ArchwayStoreCityModel>();

            var storeRecords = _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var cities = storeRecords
                .Where(r => r.State == stateProvince.Abbreviation)
                .OrderBy(r => r.City)
                .Select(r => r.City)
                .Distinct()
                .ToList();
            var result = new List<ArchwayStoreCityModel>();
            foreach (var city in cities)
            {
                result.Add(new ArchwayStoreCityModel
                {
                    name = city
                });
            }

            return result;
        }

        public IList<ArchwayStoreAddressModel> GetArchwayStoreAddressesByCity(string city, bool addSelectAddressItem)
        {
            if (string.IsNullOrWhiteSpace(city))
                return new List<ArchwayStoreAddressModel>();

            var storeRecords = _archwayStudentEmployeeRegistrationFieldService.GetArchwayStoreRecordInfos();

            var records = storeRecords
                .Where(r => r.City == city)
                .OrderBy(r => r.Address)
                .ToList();
            var result = new List<ArchwayStoreAddressModel>();
            foreach (var record in records)
            {
                result.Add(new ArchwayStoreAddressModel
                {
                    name = record.Address,
                    storeNumber = record.Id,
                    storeType = record.StoreType
                });
            }

            return result;
        }

        public IList<ArchwayStoreEmployeePositionModel> GetArchwayStoreEmployeePositionsByStore(string storeNumber, bool addSelectPositionItem)
        {
            if (string.IsNullOrWhiteSpace(storeNumber))
                return new List<ArchwayStoreEmployeePositionModel>();

            var storeRecord = _archwayStudentEmployeeRegistrationFieldService
                .GetArchwayStoreRecordInfo(int.Parse(storeNumber));

            if (storeRecord == null)
                return new List<ArchwayStoreEmployeePositionModel>();

            var employePositions =
                _archwayStudentEmployeeRegistrationFieldService
                    .GetArchwayStoreEmployeePositions(storeRecord.StoreType)
                    .OrderBy(p => p.JobTitle);

            var result = new List<ArchwayStoreEmployeePositionModel>();
            foreach (var position in employePositions)
            {
                result.Add(new ArchwayStoreEmployeePositionModel
                {
                    name = position.JobTitle
                });
            }

            return result;
        }
    }
}
