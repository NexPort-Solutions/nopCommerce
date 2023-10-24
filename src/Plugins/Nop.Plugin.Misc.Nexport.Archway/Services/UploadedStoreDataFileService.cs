using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Archway.Data;

namespace Nop.Plugin.Misc.Nexport.Archway.Services;

public interface IUploadedStoreDataFileService
{
    Task<Result<string, string>> Save(IFormFile storeDataFile);
    Task Process(string storeDataFilePath);
}

public class UploadedStoreDataFileService : IUploadedStoreDataFileService
{
    private readonly INopDataProvider _nopDataProvider;
    private readonly INopFileProvider _fileProvider;
    private readonly CsvConfiguration _csvConfiguration = new(CultureInfo.InvariantCulture) { Delimiter = "|" };

    public UploadedStoreDataFileService(INopFileProvider fileProvider, INopDataProvider nopDataProvider)
    {
        _fileProvider = fileProvider;
        _nopDataProvider = nopDataProvider;
    }

    public async Task<Result<string, string>> Save(IFormFile storeDataFile)
    {
        var fileExtension = _fileProvider.GetFileExtension(storeDataFile.FileName);
        if (!fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase)
            && !fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        {
            return Error($"Invalid file extension '{fileExtension}' for uploaded file.");
        }
        var storeDataPath = _fileProvider.GetAbsolutePath(PluginDefaults.UploadPath);
        if (!_fileProvider.DirectoryExists(storeDataPath))
        {
            _fileProvider.CreateDirectory(storeDataPath);
        }
        var uploadFolder = $"{DateTime.UtcNow:yyyyMMddhhmmssfff}";
        var fullDataPath = _fileProvider.Combine(storeDataPath, uploadFolder);
        if (!_fileProvider.DirectoryExists(fullDataPath))
        {
            _fileProvider.CreateDirectory(fullDataPath);
        }
        var csvFilePath = _fileProvider.Combine(fullDataPath, storeDataFile.FileName);
        try
        {
            using var fileStream = new FileStream(csvFilePath, FileMode.Create);
            await storeDataFile.CopyToAsync(fileStream);
            return Okay(csvFilePath);
        }
        catch
        {
            return Error($"Could not write to {csvFilePath}");
        }
    }

    public async Task Process(string storeDataFilePath)
    {
        var filePath = _fileProvider.GetAbsolutePath(storeDataFilePath);
        using var csv = new CsvReader(new StreamReader(filePath), _csvConfiguration);
        csv.Context.RegisterClassMap<StoreRecordParsingClassMap>();
        using var dt = new DataTable();
        using var dr = new CsvDataReader(csv);
        dt.Load(dr);
        if (DataSettingsManager.LoadSettings() is not { DataProvider: not DataProviderType.Unknown, ConnectionString: var connectionString } || connectionString is null or "")
        {
            return;
        }
        await _nopDataProvider.ExecuteNonQueryAsync("DELETE FROM ArchwayStore");
        using var bulkCopy = new SqlBulkCopy(connectionString) { BatchSize = 1000 };
        var map = new StoreRecordParsingClassMap();
        foreach (var member in map.MemberMaps)
        {
            bulkCopy.ColumnMappings.Add(member.Data.Names.First(), member.Data.Member.Name);
        }
        bulkCopy.DestinationTableName = "ArchwayStore";
        await bulkCopy.WriteToServerAsync(dt);
    }
}
