using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;
using Catalog = Nop.Plugin.Misc.Nexport.Services.ICatalogService;
using Catalogs = Nop.Core.PagedList<NexportApi.Model.CatalogResponseItem>;
using Syllabi = Nop.Core.PagedList<NexportApi.Model.GetSyllabiResponseItem>;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ICatalogService
{
    Task<Catalogs> FindAllCatalogs(Guid orgId, int pageIndex = 0, int pageSize = int.MaxValue);
    Task<GetCatalogCreditHoursResponse?> GetCatalogCreditHours(Guid catalogId);
    Task<GetDescriptionResponse?> GetCatalogDescription(Guid catalogId);
    Task<CatalogResponseItem?> GetCatalogDetails(Guid catalogId);
    Task<Syllabi> FindAllSyllabuses(Guid catalogId, int pageIndex = 0, int pageSize = int.MaxValue);
}

public class CatalogService : Catalog
{
    private readonly NexportApiService _nexport;
    private readonly HelperService _helper;

    public CatalogService(NexportApiService nexportApi, HelperService settings)
    {
        _nexport = nexportApi;
        _helper = settings;
    }

    public Task<CatalogResponseItem?> GetCatalogDetails(Guid catalogId) => _helper.Do(s => _nexport.GetCatalogDetails((s.Url, s.Token), catalogId));
    public Task<GetDescriptionResponse?> GetCatalogDescription(Guid catalogId) => _helper.Do(s => _nexport.GetCatalogDescription((s.Url, s.Token), catalogId));
    public Task<GetCatalogCreditHoursResponse?> GetCatalogCreditHours(Guid catalogId) => _helper.Do(s => _nexport.GetCatalogCreditHours((s.Url, s.Token), catalogId));
    public Task<Syllabi> FindAllSyllabuses(Guid catalogId, int pageIndex, int pageSize) => _helper.GetAllPaged((s, p) => _nexport.GetSyllabuses((s.Url, s.Token), catalogId, p), pageIndex, pageSize);
    public Task<Catalogs> FindAllCatalogs(Guid orgId, int pageIndex, int pageSize) => _helper.GetAllPaged((s, p) => Adapt(_nexport.GetCatalogs((s.Url, s.Token), orgId, p)), pageIndex, pageSize);

    private static async Task<Response<List<CatalogResponseItem>>> Adapt(Task<Response<CatalogResponse>> response)
    {
        var r = await response;
        return new Response<List<CatalogResponseItem>>(r.StatusCode, r.TotalRecord, r.RecordPerPage, r.CurrentPage, r.Data.Catalogs);
    }
}
