using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface INexportPluginLocalizationService
{
    //Task AddOrUpdateResourceWhenNonExisting(string resourceName, string resourceValue, string languageCulture = null);

    Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue, int languageId = 1);
}