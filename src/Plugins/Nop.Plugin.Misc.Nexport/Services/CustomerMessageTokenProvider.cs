using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class CustomerMessageTokenProvider : MessageTokenProvider
    {
        public CustomerMessageTokenProvider(
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICurrencyService currencyService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IVendorAttributeFormatter vendorAttributeFormatter,
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
            PaymentSettings paymentSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings) : base(
                catalogSettings,
                currencySettings,
                actionContextAccessor,
                addressAttributeFormatter,
                currencyService,
                customerAttributeFormatter,
                customerService,
                dateTimeHelper,
                downloadService,
                eventPublisher,
                genericAttributeService,
                languageService,
                localizationService,
                orderService,
                paymentPluginManager,
                paymentService,
                priceFormatter,
                storeContext,
                storeService,
                urlHelperFactory,
                urlRecordService,
                vendorAttributeFormatter,
                workContext,
                templatesSettings,
                paymentSettings,
                storeInformationSettings,
                taxSettings)
        {
        }

        public override IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate)
        {
            switch (messageTemplate.Name)
            {
                case "Test":
                    return new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ReturnRequestTokens };
            }

            return base.GetTokenGroups(messageTemplate);
        }
    }
}
