using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.Archway.Models;
using Nop.Web.Models.Directory;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories
{
    public interface IArchwayStudentEmployeeRegistrationFieldModelFactory
    {
        Task<ArchwayStudentEmployeeRegistrationFieldModel> PrepareArchwayStudentEmployeeRegistrationFieldModelAsync(
            int fieldId);

        Task<ArchwayStudentEmployeeRegistrationFieldOptionModel>
            PrepareArchwayStudentEmployeeRegistrationFieldOptionModelAsync(int fieldId);

        Task<IList<ArchwayStoreCityModel>> GetArchwayStoreCitiesByState(string state, bool addSelectCityItem);

        Task<IList<ArchwayStoreAddressModel>> GetArchwayStoreAddressesByCity(string city, bool addSelectAddressItem);

        Task<IList<ArchwayStoreEmployeePositionModel>> GetArchwayStoreEmployeePositionsByStore(string storeNumber,
            bool addSelectPositionItem);
    }
}
