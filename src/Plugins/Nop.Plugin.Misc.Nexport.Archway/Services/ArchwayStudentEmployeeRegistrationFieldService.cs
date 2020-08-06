using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Plugin.Misc.Nexport.Archway.Data;
using Nop.Plugin.Misc.Nexport.Archway.Domains;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Archway.Services
{
    public class ArchwayStudentEmployeeRegistrationFieldService : IArchwayStudentEmployeeRegistrationFieldService
    {
        private readonly PluginObjectContext _pluginContext;
        private readonly IRepository<ArchwayStoreRecordInfo> _archwayStoreRecordRepository;
        private readonly IRepository<ArchwayStoreEmployeePosition> _archwayStoreEmployeePositionRepository;
        private readonly IRepository<ArchwayStudentRegistrationFieldKeyMapping> _archwayStudentRegistrationFieldKeyMappingRepository;
        private readonly IRepository<ArchwayStudentRegistrationFieldAnswer> _archwayStudentRegistrationFieldAnswerRepository;
        private readonly ICacheManager _cacheManager;
        private readonly INopFileProvider _fileProvider;
        private readonly NexportService _nexportService;
        private readonly ILogger _logger;

        public ArchwayStudentEmployeeRegistrationFieldService(
            PluginObjectContext pluginContext,
            IRepository<ArchwayStoreRecordInfo> archwayStoreRecordRepository,
            IRepository<ArchwayStoreEmployeePosition> archwayStoreEmployeePositionRepository,
            IRepository<ArchwayStudentRegistrationFieldKeyMapping> archwayStudentRegistrationFieldKeyMappingRepository,
            IRepository<ArchwayStudentRegistrationFieldAnswer> archwayStudentRegistrationFieldAnswerRepository,
            ICacheManager cacheManager,
            INopFileProvider fileProvider,
            NexportService nexportService,
            ILogger logger)
        {
            _pluginContext = pluginContext;
            _archwayStoreRecordRepository = archwayStoreRecordRepository;
            _archwayStoreEmployeePositionRepository = archwayStoreEmployeePositionRepository;
            _archwayStudentRegistrationFieldKeyMappingRepository = archwayStudentRegistrationFieldKeyMappingRepository;
            _archwayStudentRegistrationFieldAnswerRepository = archwayStudentRegistrationFieldAnswerRepository;
            _cacheManager = cacheManager;
            _fileProvider = fileProvider;
            _nexportService = nexportService;
            _logger = logger;
        }

        public string SaveUploadedStoreDataFile(IFormFile storeDataFile)
        {
            if (storeDataFile == null)
                throw new ArgumentNullException(nameof(storeDataFile));

            try
            {
                var fileExtension = _fileProvider.GetFileExtension(storeDataFile.FileName);
                if (!fileExtension?.Equals(".csv", StringComparison.InvariantCultureIgnoreCase) ?? true)
                    throw new Exception("Only csv files are supported");

                var archwayStoreDataPath = _fileProvider.GetAbsolutePath(PluginDefaults.UploadPath);

                if (!_fileProvider.DirectoryExists(archwayStoreDataPath))
                    _fileProvider.CreateDirectory(archwayStoreDataPath);

                var uploadFolder = $"{DateTime.UtcNow:yyyyMMddhhmmssfff}";
                var fullDataPath = _fileProvider.Combine(archwayStoreDataPath, uploadFolder);

                if (!_fileProvider.DirectoryExists(fullDataPath))
                    _fileProvider.CreateDirectory(fullDataPath);

                var csvFilePath = _fileProvider.Combine(fullDataPath, storeDataFile.FileName);
                using (var fileStream = new FileStream(csvFilePath, FileMode.Create))
                    storeDataFile.CopyTo(fileStream);

                return csvFilePath;
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot save uploaded store data file", ex);
                throw;
            }
        }

        public void ProcessUploadedStoreDataFile(string storeDataFilePath)
        {
            try
            {
                var filePath = _fileProvider.GetAbsolutePath(storeDataFilePath);

                using (var reader = new StreamReader(filePath))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.RegisterClassMap<ArchwayStoreRecordParsingClassMap>();
                        csv.Configuration.Delimiter = "|";

                        var dt = new DataTable();

                        using (var dr = new CsvDataReader(csv))
                        {
                            dt.Load(dr);

                            var dataSettings = DataSettingsManager.LoadSettings();
                            if (!dataSettings?.IsValid ?? true)
                                return;

                            _pluginContext.ExecuteSqlCommand(new RawSqlString("DELETE FROM ArchwayStore"));

                            using (var bulkCopy = new SqlBulkCopy(dataSettings.DataConnectionString))
                            {
                                bulkCopy.BatchSize = 1000;

                                var map = new ArchwayStoreRecordParsingClassMap();

                                foreach (var member in map.MemberMaps)
                                {
                                    bulkCopy.ColumnMappings.Add(member.Data.Names.First(), member.Data.Member.Name);
                                }

                                bulkCopy.DestinationTableName = "ArchwayStore";
                                bulkCopy.WriteToServer(dt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process uploaded store data file", ex);
                throw;
            }
        }

        public ArchwayStoreRecordInfo GetArchwayStoreRecordInfoById(int id)
        {
            return id < 0
                ? null
                : _archwayStoreRecordRepository.GetById(id);
        }

        public ArchwayStoreRecordInfo GetArchwayStoreRecordInfo(int storeNumber)
        {
            return _archwayStoreRecordRepository.TableNoTracking.FirstOrDefault(s => s.StoreNumber == storeNumber);
        }

        public IList<ArchwayStoreRecordInfo> GetArchwayStoreRecordInfos()
        {
            return _cacheManager.Get(PluginDefaults.ArchwayStoreRecordAllNoPaginationCacheKey,
                () => _archwayStoreRecordRepository.TableNoTracking.ToList());
        }

        public void InsertOrUpdateArchwayStoreRecord(ArchwayStoreRecordInfo record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var currentRecord = GetArchwayStoreRecordInfo(record.StoreNumber);
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

                _archwayStoreRecordRepository.Update(currentRecord);
            }
            else
            {
                _archwayStoreRecordRepository.Insert(record);
            }
        }

        public void DeleteArchwayStoreRecord(ArchwayStoreRecordInfo record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _archwayStoreRecordRepository.Delete(record);
        }

        public ArchwayStoreEmployeePosition GetArchwayStoreEmployeePositionById(int id)
        {
            return id < 1
                ? null
                : _archwayStoreEmployeePositionRepository.GetById(id);
        }

        public IList<ArchwayStoreEmployeePosition> GetArchwayStoreEmployeePositions(string jobType)
        {
            if (string.IsNullOrWhiteSpace(jobType))
                return _cacheManager.Get(PluginDefaults.ArchwayStoreEmployeePositionAllNoPaginationCacheKey,
                    () => _archwayStoreEmployeePositionRepository
                        .TableNoTracking.ToList());

            return _cacheManager.Get(PluginDefaults.ArchwayStoreEmployeePositionAllNoPaginationCacheKey,
                () => _archwayStoreEmployeePositionRepository.
                    TableNoTracking
                    .Where(p => p.JobType == jobType).ToList());
        }

        public void InsertArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            _archwayStoreEmployeePositionRepository.Insert(position);
        }

        public void UpdateArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            _archwayStoreEmployeePositionRepository.Update(position);
        }

        public void DeleteArchwayStoreEmployeePosition(ArchwayStoreEmployeePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            _archwayStoreEmployeePositionRepository.Delete(position);
        }

        public ArchwayStudentRegistrationFieldKeyMapping GetArchwayStudentRegistrationFieldKeyMapping(int fieldId, string fieldControlName)
        {
            if (fieldId < 1 || string.IsNullOrWhiteSpace(fieldControlName))
                return null;

            return _archwayStudentRegistrationFieldKeyMappingRepository
                .TableNoTracking
                .FirstOrDefault(x =>
                    x.FieldId == fieldId && x.FieldControlName == fieldControlName);
        }

        public void InsertOrUpdateArchwayStudentRegistrationFieldKeyMapping(
            ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            var currentMapping = _archwayStudentRegistrationFieldKeyMappingRepository
                .Table.FirstOrDefault(x => x.FieldId == fieldKeyMapping.FieldId &&
                    x.FieldControlName == fieldKeyMapping.FieldControlName);

            if (currentMapping != null)
            {
                currentMapping.FieldKey = fieldKeyMapping.FieldKey;
                _archwayStudentRegistrationFieldKeyMappingRepository.Update(currentMapping);
            }
            else
            {
                _archwayStudentRegistrationFieldKeyMappingRepository.Insert(fieldKeyMapping);
            }
        }

        public void DeleteArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            _archwayStudentRegistrationFieldKeyMappingRepository.Delete(fieldKeyMapping);
        }

        public void UpdateArchwayStudentRegistrationFieldKeyMapping(ArchwayStudentRegistrationFieldKeyMapping fieldKeyMapping)
        {
            if (fieldKeyMapping == null)
                throw new ArgumentNullException(nameof(fieldKeyMapping));

            _archwayStudentRegistrationFieldKeyMappingRepository.Update(fieldKeyMapping);
        }

        public IList<ArchwayStudentRegistrationFieldAnswer> GetArchwayStudentRegistrationFieldAnswers(int customerId, int fieldId)
        {
            if (customerId < 1)
                return new List<ArchwayStudentRegistrationFieldAnswer>();

            return _archwayStudentRegistrationFieldAnswerRepository
                .TableNoTracking.Where(x =>
                    x.CustomerId == customerId && x.FieldId == fieldId).ToList();
        }

        public void InsertArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            _archwayStudentRegistrationFieldAnswerRepository.Insert(answer);
        }

        public void DeleteArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            if (_archwayStudentRegistrationFieldAnswerRepository.TableNoTracking.Any(x =>
                x.CustomerId == answer.CustomerId && x.FieldId == answer.FieldId && x.FieldKey == answer.FieldKey))
                return;

            _archwayStudentRegistrationFieldAnswerRepository.Insert(answer);
        }

        public void UpdateArchwayStudentRegistrationFieldAnswer(ArchwayStudentRegistrationFieldAnswer answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            _archwayStudentRegistrationFieldAnswerRepository.Insert(answer);
        }

        public Dictionary<string, string> ParseArchwayStoreEmployeeRegistrationFields(int fieldId, IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var result = new Dictionary<string, string>();

            if (fieldId < 1)
                return result;

            var controlId = $"{NexportDefaults.NexportRegistrationFieldPrefix}-{fieldId}.{PluginDefaults.HtmlFieldPrefix}";

            var customFieldsInForm = form.Where(x => x.Key.Contains(controlId));
            foreach (var field in customFieldsInForm)
            {
                var registrationFieldKey = field.Key.Substring(field.Key.IndexOf(PluginDefaults.HtmlFieldPrefix, StringComparison.Ordinal) +
                                                               PluginDefaults.HtmlFieldPrefix.Length + 1);
                result.Add(registrationFieldKey, field.Value);
            }

            return result;
        }

        public void SaveArchwayStoreEmployeeRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _nexportService.InsertNexportRegistrationFieldAnswer(
                new NexportRegistrationFieldAnswer
                {
                    CustomerId = customer.Id,
                    FieldId = fieldId,
                    IsCustomField = true,
                    UtcDateCreated = DateTime.UtcNow
                });

            foreach (var field in fields)
            {
                var fieldKeyMapping = GetArchwayStudentRegistrationFieldKeyMapping(fieldId, field.Key);
                if (fieldKeyMapping != null)
                {
                    InsertArchwayStudentRegistrationFieldAnswer(
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

        public Dictionary<string, string> ProcessArchwayStoreEmployeeRegistrationFields(int customerId, int fieldId)
        {
            var answers = GetArchwayStudentRegistrationFieldAnswers(customerId, fieldId);

            return answers.ToDictionary(answer => answer.FieldKey, answer => answer.TextValue);
        }
    }
}
