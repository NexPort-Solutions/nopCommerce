using System.Diagnostics.CodeAnalysis;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Services;

public class HelperService
{
    private readonly Settings _settings;

    public string? Url => _settings.Url;
    public string? AuthenticationToken => _settings.Url;
    public DateTime? UtcExpirationDate => _settings.UtcExpirationDate;
    public Guid? RootOrganizationId => _settings.RootOrganizationId;
    public Guid? MerchantAccountId => _settings.MerchantAccountId;
    public bool IgnoreAcl => _settings.IgnoreAcl;
    public bool IgnoreStoreLimitations => _settings.IgnoreStoreLimitations;

    public HelperService(Settings settings) => _settings = settings;

    [MemberNotNullWhen(true, nameof(Url))]
    [MemberNotNullWhen(true, nameof(AuthenticationToken))]
    [MemberNotNullWhen(true, nameof(UtcExpirationDate))]
    [MemberNotNullWhen(true, nameof(RootOrganizationId))]
    [MemberNotNullWhen(true, nameof(MerchantAccountId))]
    public bool IsValid()
        => Url is not null
            && AuthenticationToken is not null
            && UtcExpirationDate is not null
            && RootOrganizationId is not null
            && MerchantAccountId is not null;

    public async Task<T?> Do<T>(Func<ValidatedSettings, Task<T?>> f)
        => _settings.Validated() is { } validated ? await f(validated) : default;

    public async Task<T> Do<T>(Func<ValidatedSettings, Task<T?>> f, T defaultValue)
        => _settings.Validated() is { } validated ? await f(validated) ?? defaultValue : defaultValue;

    public async Task<T?> Do<T>(Func<ValidatedSettings, Task<Response<T>>> f)
        => _settings.Validated() is { } validated ? (await f(validated)).Data : default;

    public async Task<T> Do<T>(Func<ValidatedSettings, Task<Response<T>>> f, T defaultValue)
        => _settings.Validated() is { } validated ? (await f(validated)).Data ?? defaultValue : defaultValue;

    public Task<PagedList<T>> GetAllPaged<T>(Func<ValidatedSettings, int, Task<Response<List<T>>>> f, int pageIndex, int pageSize) => Do(async s =>
    {
        var items = new List<T>();
        var page = 1;
        int remainderItemsCount;
        do
        {
            var result = await f(s, page);
            items.AddRange(result.Data);
            remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
            page++;
        } while (remainderItemsCount > -1);
        return new PagedList<T>(items, pageIndex, pageSize);
    }, new PagedList<T>(new List<T>(), pageIndex, pageSize));

    public Task<List<T>> GetAll<T>(Func<ValidatedSettings, int, Task<Response<List<T>>>> f) => Do(async s =>
    {
        var items = new List<T>();
        var page = 1;
        int remainderItemsCount;
        do
        {
            var result = await f(s, page);
            items.AddRange(result.Data);
            remainderItemsCount = result.TotalRecord - (result.RecordPerPage * page);
            page++;
        } while (remainderItemsCount > -1);
        return items;
    }, new List<T>());
}
