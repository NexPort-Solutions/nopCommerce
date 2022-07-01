using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public interface IRegistrationFieldCustomRender : IPlugin
    {
        /// <summary>
        /// Get URL for custom render option
        /// </summary>
        /// <param name="fieldId">Registration field identifier</param>
        /// <returns>URL</returns>
        string GetRenderOptionUrl(int fieldId);

        string GetCustomRenderUrl(int fieldId);

        string GetCustomFieldPrefix();

        Task<Dictionary<string, string>> ParseCustomRegistrationFields(int fieldId, IFormCollection form);

        Task SaveCustomRegistrationFields(int fieldId, Dictionary<string, string> fields);

        Task<Dictionary<string, string>> ProcessCustomRegistrationFields(int customerId, int fieldId);
    }
}
