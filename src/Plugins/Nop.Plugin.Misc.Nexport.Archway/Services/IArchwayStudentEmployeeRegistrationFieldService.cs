using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Archway.Domains;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public interface IArchwayStudentEmployeeRegistrationFieldService
    {
        string SaveUploadedStoreDataFile(IFormFile storeDataFile);

        void ProcessUploadedStoreDataFile(string storeDataFilePath);

        ArchwayStoreRecordInfo GetArchwayStoreRecordInfo(int storeNumber);

        IList<ArchwayStoreRecordInfo> GetArchwayStoreRecordInfos();

        void InsertOrUpdateArchwayStoreRecord(ArchwayStoreRecordInfo record);

        void DeleteArchwayStoreRecord(ArchwayStoreRecordInfo record);

        ArchwayStoreEmployeePosition GetArchwayStoreEmployeePositionById(int id);

        IList<ArchwayStoreEmployeePosition> GetArchwayStoreEmployeePositions(string jobType);

        void InsertArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        void UpdateArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        void DeleteArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position);

        ArchwayStudentRegistrationFieldKeyMapping GetArchwayStudentRegistrationFieldKeyMapping(string fieldControlName);

        void InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping);

        void DeleteArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping);

        IList<ArchwayStudentRegistrationFieldAnswer> GetArchwayStudentRegistrationFieldAnswers(int customerId, int fieldId);

        void InsertArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        void DeleteArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        void UpdateArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer);

        Dictionary<string, string> ParseArchwayStoreEmployeeRegistrationFields(int fieldId, IFormCollection form);

        void SaveArchwayStoreEmployeeRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields);

        Dictionary<string, string> ProcessArchwayStoreEmployeeRegistrationFields(int customerId, int fieldId);
    }
}
