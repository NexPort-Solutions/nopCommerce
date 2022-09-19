using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services
{
    public class PluginLocalizationService : IPluginLocalizationService
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IRepository<LocaleStringResource> _lsrRepository;

        public PluginLocalizationService(ILocalizationService localizationService, 
            ILanguageService languageService,
            INotificationService notificationService,
            IRepository<LocaleStringResource> lsrRepository)
        {
            _localizationService = localizationService;
            _languageService = languageService;
            _lsrRepository = lsrRepository;
        }

         /// <summary>
        /// 
        /// </summary>  
        /// <param name="resourceName"></param>
        /// <param name="resourceValue"></param>
        /// <param name="languageCulture"></param>
        /// <returns></returns>
        public async Task AddOrUpdateResourceWhenNonExisting(string resourceName, string resourceValue, string languageCulture = null)
        {
            foreach (var lang in await _languageService.GetAllLanguagesAsync(true))
            {
                if (!string.IsNullOrEmpty(languageCulture) && !languageCulture.Equals(lang.LanguageCulture))
                    continue;

                var lsr = await _localizationService.GetLocaleStringResourceByNameAsync(resourceName, lang.Id, false);
                if (lsr == null)
                {
                    lsr = new LocaleStringResource
                    {
                        LanguageId = lang.Id,
                        ResourceName = resourceName,
                        ResourceValue = resourceValue
                    };
                    await _localizationService.InsertLocaleStringResourceAsync(lsr);
                }
            }
            
        }

        public async Task<bool> CheckForExistingResourceAndAddNonExistingResource(string resourceName, string resourceValue,
            int languageId = 1)
        {
            var query = from lsr in _lsrRepository.Table
                orderby lsr.ResourceName
                where lsr.LanguageId == languageId && lsr.ResourceName == resourceName.ToLowerInvariant()
                select lsr; 

            var localeStringResource = await query.FirstOrDefaultAsync();

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

            // if there is a locale resource thats modified we return true
            return localeStringResource.ResourceValue != resourceValue;
        }
    }
}
