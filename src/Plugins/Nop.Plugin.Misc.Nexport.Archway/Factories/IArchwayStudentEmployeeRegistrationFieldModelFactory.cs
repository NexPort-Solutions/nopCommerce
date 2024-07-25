using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.Archway.Models;

namespace Nop.Plugin.Misc.Nexport.Archway.Factories;

public interface IArchwayStudentEmployeeRegistrationFieldModelFactory
{
    Task<ArchwayPluginResourceListModel> PrepareArchwayPluginResourceListModelAsync(
        ArchwayPluginResourceListSearchModel searchModel);

    Task<ArchwayStudentEmployeeRegistrationFieldModel> PrepareArchwayStudentEmployeeRegistrationFieldModelAsync(
        int fieldId, bool renderAdminView);

    Task<ArchwayStudentEmployeeRegistrationFieldOptionModel>
        PrepareArchwayStudentEmployeeRegistrationFieldOptionModelAsync(int fieldId);

    Task<ArchwayStudentEmployeeRegistrationFieldModel> PrepareEditArchwayStudentEmployeeRegistrationFieldModel(
        int customerId, int fieldId);

    Task<IList<ArchwayStoreCityModel>> GetArchwayStoreCitiesByState(string state, bool addSelectAddressItem);

    Task<IList<ArchwayStoreAddressModel>> GetArchwayStoreAddressesByCity(string city, string state, bool addSelectAddressItem);

    Task<IList<ArchwayStoreEmployeePositionModel>> GetArchwayStoreEmployeePositionsByStore(string storeNumber,
        bool addSelectPositionItem);
}