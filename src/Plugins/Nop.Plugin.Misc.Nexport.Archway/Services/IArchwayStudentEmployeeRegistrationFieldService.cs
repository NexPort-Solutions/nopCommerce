using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public interface IArchwayStudentEmployeeRegistrationFieldService
    {
        Task<string> SaveUploadedStoreDataFile(IFormFile storeDataFile);

        Task ProcessUploadedStoreDataFile(string storeDataFilePath);

        Task<ArchwayStoreRecordInfo> GetArchwayStoreRecordInfo(int storeNumber);

        Task<IList<ArchwayStoreRecordInfo>> GetArchwayStoreRecordInfos();

        Task InsertOrUpdateArchwayStoreRecord(ArchwayStoreRecordInfo record);

        Task DeleteArchwayStoreRecord(ArchwayStoreRecordInfo record);

        Task<ArchwayStoreEmployeePosition> GetArchwayStoreEmployeePositionById(int id);

        Task<IList<ArchwayStoreEmployeePosition>> GetArchwayStoreEmployeePositions(string jobType);

        Task InsertArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        Task UpdateArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        Task DeleteArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        Task<ArchwayStudentRegistrationFieldKeyMapping> GetArchwayStudentRegistrationFieldKeyMapping(string fieldControlName);

        Task InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping);

        Task DeleteArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping);

        Task<IList<ArchwayStudentRegistrationFieldAnswer>> GetArchwayStudentRegistrationFieldAnswers(int customerId, int fieldId);

        Task InsertArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        Task DeleteArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        Task UpdateArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        Task<Dictionary<string, string>> ParseArchwayStoreEmployeeRegistrationFields(int fieldId, IFormCollection form);

        Task SaveArchwayStoreEmployeeRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields);

        Task<Dictionary<string, string>> ProcessArchwayStoreEmployeeRegistrationFields(int customerId, int fieldId);
    }
}
