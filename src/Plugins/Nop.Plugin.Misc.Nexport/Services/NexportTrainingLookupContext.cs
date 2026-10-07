using Microsoft.AspNetCore.Http;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Organization;

namespace Nop.Plugin.Misc.Nexport.Services;

// Created for one sequential training-model build, never registered in DI or retained across requests.
internal sealed class NexportTrainingLookupContext
{
    private readonly Dictionary<Guid, InvoiceRedemptionResponse> _redemptions = new();
    private readonly Dictionary<(string Url, string Token, Guid OrganizationId, int? Page), NexportOrganizationResponse>
        _organizationPages = new();

    internal async Task<InvoiceRedemptionResponse> GetRedemptionAsync(Guid invoiceItemId,
        Func<Task<InvoiceRedemptionResponse>> load, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_redemptions.TryGetValue(invoiceItemId, out var cached))
            return cached;

        var result = await load();
        cancellationToken.ThrowIfCancellationRequested();
        if (result?.ApiErrorEntity?.ErrorCode == 0)
            _redemptions.Add(invoiceItemId, result);

        return result;
    }

    internal async Task<NexportOrganizationResponse> GetOrganizationsAsync(string url, string token,
        Guid organizationId, int? page, Func<Task<NexportOrganizationResponse>> load,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = (url, token, organizationId, page);
        if (_organizationPages.TryGetValue(key, out var cached))
            return cached;

        var result = await load();
        cancellationToken.ThrowIfCancellationRequested();
        if (result?.StatusCode == StatusCodes.Status200OK &&
            (result.OrganizationList == null || result.OrganizationList.All(
                organization => organization.ApiErrorEntity == null || organization.ApiErrorEntity.ErrorCode == 0)))
            _organizationPages.Add(key, result);

        return result;
    }
}