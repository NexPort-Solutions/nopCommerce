using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Services.Localization;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

public class PluginLocalizationService : IPluginLocalizationService
{
    private readonly ILocalizationService _localizationService;

    public PluginLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue,
        int languageId = 1)
    {
        var localeStringResource = await _localizationService.GetLocaleStringResourceByNameAsync(resourceName, languageId, false);

        if (localeStringResource == null)
        {
            var newLsr = new LocaleStringResource
            {
                LanguageId = languageId,
                ResourceName = resourceName,
                ResourceValue = resourceValue
            };

            await _localizationService.InsertLocaleStringResourceAsync(newLsr);

            return false;
        }

        return localeStringResource.ResourceValue != resourceValue;
    }
}