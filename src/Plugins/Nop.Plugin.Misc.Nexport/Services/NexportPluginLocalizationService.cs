using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.Services;

public class NexportPluginLocalizationService : INexportPluginLocalizationService
{
    private readonly ILocalizationService _localizationService;

    public NexportPluginLocalizationService(
        ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue, int languageId = 1)
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