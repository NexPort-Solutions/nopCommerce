using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using LinqToDB.Common;
using LinqToDB.DataProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Archway.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Localization;
using Nop.Services.Caching;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public class ArchwayStudentEmployeeRegistrationFieldService : IArchwayStudentEmployeeRegistrationFieldService
    {
        private readonly INopDataProvider _nopDataProvider;
        private readonly IRepository<ArchwayStoreRecordInfo> _archwayStoreRecordRepository;
        private readonly IRepository<ArchwayStoreEmployeePosition> _archwayStoreEmployeePositionRepository;
        private readonly IRepository<ArchwayStudentRegistrationFieldKeyMapping> _archwayStudentRegistrationFieldKeyMappingRepository;
        private readonly IRepository<ArchwayStudentRegistrationFieldAnswer> _archwayStudentRegistrationFieldAnswerRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly NexportService _nexportService;
        private readonly ILogger _logger;

        public ArchwayStudentEmployeeRegistrationFieldService(
            IRepository<ArchwayStoreRecordInfo> archwayStoreRecordRepository,
            IRepository<ArchwayStoreEmployeePosition> archwayStoreEmployeePositionRepository,
            IRepository<ArchwayStudentRegistrationFieldKeyMapping> archwayStudentRegistrationFieldKeyMappingRepository,
            IRepository<ArchwayStudentRegistrationFieldAnswer> archwayStudentRegistrationFieldAnswerRepository,
            IStaticCacheManager cacheManager,
            INopFileProvider fileProvider,
            INopDataProvider nopDataProvider,
            NexportService nexportService,
            ILogger logger)
        {
            _archwayStoreRecordRepository = archwayStoreRecordRepository;
            _archwayStoreEmployeePositionRepository = archwayStoreEmployeePositionRepository;
            _archwayStudentRegistrationFieldKeyMappingRepository = archwayStudentRegistrationFieldKeyMappingRepository;
            _archwayStudentRegistrationFieldAnswerRepository = archwayStudentRegistrationFieldAnswerRepository;
            _cacheManager = cacheManager;
            _fileProvider = fileProvider;
            _nopDataProvider = nopDataProvider;
            _nexportService = nexportService;
            _logger = logger;
        }

        public async Task<string> SaveUploadedStoreDataFile(IFormFile storeDataFile)
        {
            if (storeDataFile == null)
                throw new ArgumentNullException(nameof(storeDataFile));

            try
            {
                var fileExtension = _fileProvider.GetFileExtension(storeDataFile.FileName);
                if (fileExtension == null || (!fileExtension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase) &&
                                              !fileExtension.Equals(".txt", StringComparison.InvariantCultureIgnoreCase)))
                    throw new Exception("Only csv or txt files are supported");

                var archwayStoreDataPath = _fileProvider.GetAbsolutePath(PluginDefaults.UploadPath);

                if (!_fileProvider.DirectoryExists(archwayStoreDataPath))
                    _fileProvider.CreateDirectory(archwayStoreDataPath);

                var uploadFolder = $"{DateTime.UtcNow:yyyyMMddhhmmssfff}";
                var fullDataPath = _fileProvider.Combine(archwayStoreDataPath, uploadFolder);

                if (!_fileProvider.DirectoryExists(fullDataPath))
                    _fileProvider.CreateDirectory(fullDataPath);

                var csvFilePath = _fileProvider.Combine(fullDataPath, storeDataFile.FileName);
                await using var fileStream = new FileStream(csvFilePath, FileMode.Create);
                await storeDataFile.CopyToAsync(fileStream);

                return csvFilePath;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot save uploaded store data file", ex);
                throw;
            }
        }

        public async Task ProcessUploadedStoreDataFile(string storeDataFilePath)
        {
            try
            {
                var filePath = _fileProvider.GetAbsolutePath(storeDataFilePath);

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|"
                });
                csv.Context.RegisterClassMap<ArchwayStoreRecordParsingClassMap>();

                var dt = new DataTable();

                using var dr = new CsvDataReader(csv);
                dt.Load(dr);

                var dataSettings = DataSettingsManager.LoadSettings();
                if (dataSettings == null ||
                    dataSettings.DataProvider == DataProviderType.Unknown ||
                    string.IsNullOrWhiteSpace(dataSettings.ConnectionString))
                    return;

                await _nopDataProvider.ExecuteNonQueryAsync("DELETE FROM ArchwayStore");

                using var bulkCopy = new SqlBulkCopy(dataSettings.ConnectionString) { BatchSize = 1000 };

                var map = new ArchwayStoreRecordParsingClassMap();

                foreach (var member in map.MemberMaps)
                {
                    bulkCopy.ColumnMappings.Add(member.Data.Names.First(), member.Data.Member.Name);
                }

                bulkCopy.DestinationTableName = "ArchwayStore";
                await bulkCopy.WriteToServerAsync(dt);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Cannot process uploaded store data file", ex);
                throw;
            }
        }

        public async Task<ArchwayStoreRecordInfo> GetArchwayStoreRecordInfoById(int id)
        {
            return id < 0
                ? null
                : await _archwayStoreRecordRepository.GetByIdAsync(id);
        }

        public async Task<ArchwayStoreRecordInfo> GetArchwayStoreRecordInfo(int storeNumber)
        {
            return await _archwayStoreRecordRepository.Table.FirstOrDefaultAsync(s => s.StoreNumber == storeNumber);
        }

        public async Task<IList<ArchwayStoreRecordInfo>> GetArchwayStoreRecordInfos()
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(PluginDefaults.ArchwayStoreRecordAllNoPaginationCacheKey);

            return await _cacheManager.GetAsync(cacheKey, () => _archwayStoreRecordRepository.Table.ToListAsync());
        }

        public async Task InsertOrUpdateArchwayStoreRecord(ArchwayStoreRecordInfo record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var currentRecord = await GetArchwayStoreRecordInfo(record.StoreNumber);
            if (currentRecord != null)
            {
                currentRecord.StoreNumber = record.StoreNumber;
                currentRecord.OperatorId = record.OperatorId;
                currentRecord.RegionCode = record.RegionCode;
                currentRecord.Address = record.Address;
                currentRecord.City = record.City;
                currentRecord.State = record.State;
                currentRecord.PostalCode = record.PostalCode;
                currentRecord.AdvertisingCoop = record.AdvertisingCoop;
                currentRecord.StoreType = record.StoreType;
                currentRecord.OperatorFirstName = record.OperatorFirstName;
                currentRecord.OperatorLastName = record.OperatorLastName;

                await _archwayStoreRecordRepository.UpdateAsync(currentRecord);
            }
            else
            {
                await _archwayStoreRecordRepository.InsertAsync(record);
            }
        }

        public async Task DeleteArchwayStoreRecord(ArchwayStoreRecordInfo record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await _archwayStoreRecordRepository.DeleteAsync(record);
        }

        public async Task<ArchwayStoreEmployeePosition> GetArchwayStoreEmployeePositionById(int id)
        {
            return id < 1
                ? null
                : await _archwayStoreEmployeePositionRepository.GetByIdAsync(id);
        }

        public async Task<IList<ArchwayStoreEmployeePosition>> GetArchwayStoreEmployeePositions(string jobType)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(PluginDefaults.ArchwayStoreEmployeePositionAllNoPaginationCacheKey);

            if (string.IsNullOrWhiteSpace(jobType))
                return await _cacheManager.GetAsync(cacheKey, () => _archwayStoreEmployeePositionRepository.Table.ToListAsync());

            return await _cacheManager.GetAsync(cacheKey,
                () => _archwayStoreEmployeePositionRepository.Table.Where(p => p.JobType == jobType).ToListAsync());
        }

        public async Task InsertArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            await _archwayStoreEmployeePositionRepository.InsertAsync(position);
        }

        public async Task UpdateArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            await _archwayStoreEmployeePositionRepository.UpdateAsync(position);
        }

        public async Task DeleteArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            await _archwayStoreEmployeePositionRepository.DeleteAsync(position);
        }

        public  async Task<ArchwayStudentRegistrationFieldKeyMapping> GetArchwayStudentRegistrationFieldKeyMapping(
            string fieldControlName)
        {
            if (string.IsNullOrWhiteSpace(fieldControlName))
                return null;

            return await _archwayStudentRegistrationFieldKeyMappingRepository.Table
                .FirstOrDefaultAsync(x => x.FieldControlName == fieldControlName);
        }

        public ArchwayStudentRegistrationFieldKeyMapping GetArchwayStudentRegistrationFieldKeyMappingByFieldKey(string fieldKey)
        {
            if (string.IsNullOrWhiteSpace(fieldKey))
                return null;

            return _archwayStudentRegistrationFieldKeyMappingRepository
                .Table
                .FirstOrDefault(x => x.FieldKey == fieldKey);
        }

        public async Task InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(
            ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            var currentMapping = _archwayStudentRegistrationFieldKeyMappingRepository
                .Table.FirstOrDefault(x => x.FieldControlName == fieldKeyMapping.FieldControlName);

            if (currentMapping != null)
            {
                currentMapping.FieldKey = fieldKeyMapping.FieldKey;
                await _archwayStudentRegistrationFieldKeyMappingRepository.UpdateAsync(currentMapping);
            }
            else
            {
                await _archwayStudentRegistrationFieldKeyMappingRepository.InsertAsync(fieldKeyMapping);
            }
        }

        public async Task DeleteArchwayStudentRegistrationFieldKeyMapping(
            ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            await _archwayStudentRegistrationFieldKeyMappingRepository.DeleteAsync(fieldKeyMapping);
        }

        public async Task UpdateArchwayStudentRegistrationFieldKeyMapping(
            ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            await _archwayStudentRegistrationFieldKeyMappingRepository.UpdateAsync(fieldKeyMapping);
        }

        public async Task<IList<ArchwayStudentRegistrationFieldAnswer>> GetArchwayStudentRegistrationFieldAnswers(
            int customerId, int fieldId)
        {
            if (customerId < 1)
                return new List<ArchwayStudentRegistrationFieldAnswer>();

            return await _archwayStudentRegistrationFieldAnswerRepository.Table
                .Where(x => x.CustomerId == customerId && x.FieldId == fieldId).ToListAsync();
        }

        public async Task<ArchwayStudentRegistrationFieldAnswer> GetArchwayStudentRegistrationFieldAnswer(int id)
        {
            return id < 1
                ? null
                : await _archwayStudentRegistrationFieldAnswerRepository.GetByIdAsync(id);
        }

        public async Task InsertArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            await _archwayStudentRegistrationFieldAnswerRepository.InsertAsync(answer);
        }

        public async Task DeleteArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            if (await _archwayStudentRegistrationFieldAnswerRepository.Table
                .AnyAsync(x =>
                    x.CustomerId == answer.CustomerId &&
                    x.FieldId == answer.FieldId &&
                    x.FieldKey == answer.FieldKey))
                return;

            await _archwayStudentRegistrationFieldAnswerRepository.DeleteAsync(answer);
        }

        public async Task UpdateArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            await _archwayStudentRegistrationFieldAnswerRepository.UpdateAsync(answer);
        }

        public async Task UpdateArchwayStudentRegistrationFieldAnswersForCustomer(int customerId, int fieldId,
            Dictionary<string, string> fields)
        {
            var answers = await GetArchwayStudentRegistrationFieldAnswers(customerId, fieldId);
            var prefix = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";
            foreach (var (key, value) in fields)
            {
                var fieldControl = key[(key.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length + 1)..];
                var fieldKeyMapping = await GetArchwayStudentRegistrationFieldKeyMapping(fieldControl);
                if (fieldKeyMapping != null)
                {
                    var currentAnswer = answers.FirstOrDefault(x => x.FieldKey == fieldKeyMapping.FieldKey);
                    if (currentAnswer != null)
                    {
                        currentAnswer.TextValue = value;
                        currentAnswer.UtcDateModified = DateTime.UtcNow;

                        await UpdateArchwayStudentRegistrationFieldAnswer(currentAnswer);
                    }
                    else
                    {
                        var newAnswer = new ArchwayStudentRegistrationFieldAnswer
                        {
                            CustomerId = customerId,
                            FieldId = fieldId,
                            FieldKey = fieldKeyMapping.FieldKey,
                            TextValue = value,
                            UtcDateCreated = DateTime.UtcNow,
                            UtcDateModified = DateTime.UtcNow
                        };

                        await InsertArchwayStudentRegistrationFieldAnswer(newAnswer);
                    }
                }
            }
        }

        public Task<Dictionary<string, string>> ParseArchwayStoreEmployeeRegistrationFields(int fieldId,
            IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<string, string>();

            if (fieldId < 1)
                return Task.FromResult(result);

            var controlId = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";

            var customFieldsInForm = form.Where(x => x.Key.Contains(controlId));
            foreach (var (key, value) in customFieldsInForm)
            {
                var registrationFieldKey = key[(key.IndexOf(PluginDefaults.HtmlFieldPrefix, StringComparison.Ordinal) +
                                                PluginDefaults.HtmlFieldPrefix.Length + 1)..];
                result.Add(registrationFieldKey, value);
            }

            return Task.FromResult(result);
        }

        public async Task SaveArchwayStoreEmployeeRegistrationFields(Customer customer, int fieldId,
            Dictionary<string, string> fields)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            try
            {
                if (fields.Count > 0)
                {
                    var includeStoreIdField = fields.TryGetValue("StoreNumber", out var storeIdFieldValue);
                    if (!includeStoreIdField || string.IsNullOrWhiteSpace(storeIdFieldValue))
                        return;

                    int.TryParse(storeIdFieldValue, out var storeId);
                    if (storeId < 0)
                        return;

                    var includeEmployeePositionField =
                        fields.TryGetValue("EmployeePosition", out var employeePositionField);
                    if (!includeEmployeePositionField || string.IsNullOrWhiteSpace(employeePositionField))
                        return;

                    await _nexportService.InsertNexportRegistrationFieldAnswer(
                        new NexportRegistrationFieldAnswer
                        {
                            CustomerId = customer.Id,
                            FieldId = fieldId,
                            IsCustomField = true,
                            UtcDateCreated = DateTime.UtcNow
                        });

                    foreach (var field in fields)
                    {
                        var fieldKeyMapping = await GetArchwayStudentRegistrationFieldKeyMapping(field.Key);
                        if (fieldKeyMapping != null)
                        {
                            await InsertArchwayStudentRegistrationFieldAnswer(
                                new ArchwayStudentRegistrationFieldAnswer
                                {
                                    CustomerId = customer.Id,
                                    FieldId = fieldId,
                                    FieldKey = fieldKeyMapping.FieldKey,
                                    TextValue = field.Value,
                                    UtcDateCreated = DateTime.UtcNow
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Unable to save registration field for Archway store employee", ex, customer);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> ProcessArchwayStoreEmployeeRegistrationFields(int customerId,
            int fieldId)
        {
            var answers = await GetArchwayStudentRegistrationFieldAnswers(customerId, fieldId);

            return answers.ToDictionary(answer => answer.FieldKey, answer => answer.TextValue);
        }

        public async Task<Dictionary<string, string>> GetCustomFieldNamesAndValues(int customerId, int fieldId)
        {
            var result = new Dictionary<string, string>();
            var answers = await GetArchwayStudentRegistrationFieldAnswers(customerId, fieldId);
            foreach (var answer in answers.Where(x=>x.FieldKey != "StoreIdField" && x.FieldKey != "StoreTypeField"))
            {
                var fieldKeyInfo = GetArchwayStudentRegistrationFieldKeyMappingByFieldKey(answer.FieldKey);
                if (fieldKeyInfo != null)
                {
                    result.Add(await _localizationService.GetResourceAsync($"Plugins.Misc.Nexport.Archway.Field.{fieldKeyInfo.FieldControlName}"), answer.TextValue);
                }
            }

            return result;
        }
    }
}
