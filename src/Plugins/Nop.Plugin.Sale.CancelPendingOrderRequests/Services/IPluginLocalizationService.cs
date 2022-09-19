using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public interface IPluginLocalizationService
    {

        Task AddOrUpdateResourceWhenNonExisting(string resourceName, string resourceValue, string languageCulture = null);

        Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue, int languageId = 1);
    }
}
