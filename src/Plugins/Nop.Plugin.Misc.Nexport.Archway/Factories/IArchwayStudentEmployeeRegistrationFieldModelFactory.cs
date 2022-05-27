using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Archway.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories
{
    public interface IArchwayStudentEmployeeRegistrationFieldModelFactory
    {
        ArchwayStudentEmployeeRegistrationFieldModel PrepareArchwayStudentEmployeeRegistrationFieldModel(int fieldId, bool renderAdminView);

        ArchwayStudentEmployeeRegistrationFieldOptionModel PrepareArchwayStudentEmployeeRegistrationFieldOptionModel(int fieldId);

        ArchwayStudentEmployeeRegistrationFieldModel PrepareEditArchwayStudentEmployeeRegistrationFieldModel(int customerId, int fieldId);

        IList<ArchwayStoreCityModel> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem);

        IList<ArchwayStoreAddressModel> GetArchwayStoreAddressesByCity(string city, string state, bool addSelectAddressItem);

        IList<ArchwayStoreEmployeePositionModel> GetArchwayStoreEmployeePositionsByStore(string storeNumber, bool addSelectPositionItem);
    }
}
