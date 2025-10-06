namespace Nop.Plugin.Sale.PurchaseForCustomer.Services;

public interface IPluginLocalizationService
{
    Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue, int languageId = 1);
}