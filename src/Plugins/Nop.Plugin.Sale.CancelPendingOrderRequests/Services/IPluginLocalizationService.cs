using System.Threading.Tasks;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

public interface IPluginLocalizationService
{
    Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue, int languageId = 1);
}