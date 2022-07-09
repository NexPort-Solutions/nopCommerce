using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public interface IRegistrationFieldCustomRender : IPlugin
    {
        /// <summary>
        /// Get the URL for custom render option MVC action
        /// </summary>
        /// <param name="fieldId">Registration field identifier</param>
        /// <returns>URL for the custom render option MVC action</returns>
        string GetRenderOptionUrl(int fieldId);

        /// <summary>
        /// Get the URL for custom render MVC action
        /// </summary>
        /// <param name="fieldId">Registration field identifier</param>
        /// <param name="renderAdminView">Determine if UI within admin area is rendered</param>
        /// <returns>URL for the custom render MVC action</returns>
        Task<string> GetCustomRenderUrl(int fieldId, bool renderAdminView);

        /// <summary>
        /// Get the custom field prefix
        /// </summary>
        /// <returns>The prefix for custom field</returns>
        string GetCustomFieldPrefix();

        /// <summary>
        /// Parse and return collection of custom registration fields from the form data
        /// </summary>
        /// <param name="fieldId">Registration field identifier</param>
        /// <param name="form">The form data</param>
        /// <returns>Dictionary of custom registration fields</returns>
        Task<Dictionary<string, string>> ParseCustomRegistrationFields(int fieldId, IFormCollection form);

        /// <summary>
        /// Save all the custom registration fields
        /// </summary>
        /// <param name="customer">The customer that will be associated with those registration fields</param>
        /// <param name="fieldId">Registration field identifier</param>
        /// <param name="fields">Dictionary of custom registration fields</param>
        Task SaveCustomRegistrationFields(Customer customer, int fieldId, Dictionary<string, string> fields);

        /// <summary>
        /// Process the registration field data in order to send it back as custom profile field in Nexport
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="fieldId">Registration field identifier</param>
        /// <returns>Dictionary of custom registration fields that have been processed based on the custom profile fields in Nexport</returns>
        Task<Dictionary<string, string>> ProcessCustomRegistrationFields(int customerId, int fieldId);

        /// <summary>
        /// Get all the custom field's name and value
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="fieldId">Registration field identifier</param>
        /// <returns>Dictionary of name and value for each custom field</returns>
        Task<Dictionary<string, string>> GetCustomFieldNamesAndValues(int customerId, int fieldId);

        /// <summary>
        /// Get the URL for the editing customer custom registration fields MVC action
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="fieldId">Registration field identifier</param>
        /// <returns>URL for the editing customer custom registration fields MVC action</returns>
        string GetEditCustomerRegistrationFieldAnswersViewUrl(int customerId, int fieldId);

        /// <summary>
        /// Update all the custom registration field answers for the customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="fieldId">Registration field identifier</param>
        /// <param name="fields">The registration fields to be updated</param>
        Task UpdateCustomRegistrationFieldAnswers(int customerId, int fieldId, Dictionary<string, string> fields);
    }
}
