using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.Nexport.Services;

public class CustomerMessageTokenProvider(
    CatalogSettings catalogSettings,
    CurrencySettings currencySettings,
    IActionContextAccessor actionContextAccessor,
    IAddressService addressService,
    IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
    IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter,
    IAttributeFormatter<VendorAttribute, VendorAttributeValue> vendorAttributeFormatter,
    IBlogService blogService,
    ICountryService countryService,
    ICurrencyService currencyService,
    ICustomerService customerService,
    IDateTimeHelper dateTimeHelper,
    IEventPublisher eventPublisher,
    IGenericAttributeService genericAttributeService,
    IGiftCardService giftCardService,
    IHtmlFormatter htmlFormatter,
    ILanguageService languageService,
    ILocalizationService localizationService,
    ILogger logger,
    INewsService newsService,
    IOrderService orderService,
    IPaymentPluginManager paymentPluginManager,
    IPaymentService paymentService,
    IPriceFormatter priceFormatter,
    IProductService productService,
    IRewardPointService rewardPointService,
    IShipmentService shipmentService,
    IStateProvinceService stateProvinceService,
    IStoreContext storeContext,
    IStoreService storeService,
    IUrlHelperFactory urlHelperFactory,
    IUrlRecordService urlRecordService,
    IWorkContext workContext,
    MessageTemplatesSettings templatesSettings,
    PaymentSettings paymentSettings,
    StoreInformationSettings storeInformationSettings,
    TaxSettings taxSettings)
    : MessageTokenProvider(catalogSettings, currencySettings, actionContextAccessor,
        addressService, addressAttributeFormatter, customerAttributeFormatter, vendorAttributeFormatter,
        blogService, countryService, currencyService, customerService, dateTimeHelper, eventPublisher,
        genericAttributeService, giftCardService, htmlFormatter, languageService, localizationService, logger,
        newsService, orderService, paymentPluginManager, paymentService, priceFormatter, productService,
        rewardPointService, shipmentService, stateProvinceService, storeContext, storeService,
        urlHelperFactory, urlRecordService, workContext, templatesSettings, paymentSettings, storeInformationSettings,
        taxSettings)
{
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