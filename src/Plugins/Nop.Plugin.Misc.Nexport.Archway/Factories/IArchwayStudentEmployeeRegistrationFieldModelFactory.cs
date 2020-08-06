using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Web.Models.Directory;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories
{
    public interface IArchwayStudentEmployeeRegistrationFieldModelFactory
    {
        ArchwayStudentEmployeeRegistrationFieldModel PrepareArchwayStudentEmployeeRegistrationFieldModel(int fieldId);

        ArchwayStudentEmployeeRegistrationFieldOptionModel PrepareArchwayStudentEmployeeRegistrationFieldOptionModel(int fieldId);

        IList<ArchwayStoreCityModel> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem);

        IList<ArchwayStoreAddressModel> GetArchwayStoreAddressesByCity(string city, bool addSelectAddressItem);

        IList<ArchwayStoreEmployeePositionModel> GetArchwayStoreEmployeePositionsByStore(string storeNumber, bool addSelectPositionItem);
    }
}
