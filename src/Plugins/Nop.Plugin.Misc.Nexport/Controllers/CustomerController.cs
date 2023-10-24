using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NexportApi.Client;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Core.Infrastructure;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using System.Globalization;
using Nop.Core.Domain.Forums;
using System.Text.Encodings.Web;
using Nop.Services.Media;
using Nop.Services.ExportImport;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Security;
using Nop.Services.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Controllers;

[Route(template: "[controller]/[action]", Order = int.MinValue)]
public class CustomerController : Web.Controllers.CustomerController
{
    #region Fields
    private readonly CaptchaSettings _captchaSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly GdprSettings _gdprSettings;
    private readonly IAddressService _address;
    private readonly IAuthenticationService _authentication;
    private readonly ICountryService _country;
    private readonly ICustomerActivityService _customerActivity;
    private readonly ICustomerAttributeParser _customerAttributeParser;
    private readonly ICustomerAttributeService _customerAttribute;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly Services.ICustomerRegistrationService _customerRegistration;
    private readonly ICustomerService _customer;
    private readonly IEventPublisher _eventPublisher;
    private readonly IGdprService _gdpr;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly ILocalizationService _localization;
    private readonly ILogger _logger;
    private readonly INewsLetterSubscriptionService _newsLetterSubscription;
    private readonly IShoppingCartService _shoppingCart;
    private readonly IStateProvinceService _stateProvince;
    private readonly IStoreContext _storeContext;
    private readonly ITaxService _tax;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessage;
    private readonly LocalizationSettings _localizationSettings;
    private readonly TaxSettings _taxSettings;
    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly IPluginModelFactory _model;
    private readonly IRegistrationFieldService _registrationField;
    #endregion Fields

    #region Ctor
    public CustomerController(
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        IPluginModelFactory model,
        IRegistrationFieldService registrationField,
        AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        HtmlEncoder htmlEncoder,
        IAddressAttributeParser addressAttributeParser,
        IAddressModelFactory addressModelFactory,
        IAddressService address,
        IAuthenticationService authentication,
        ICountryService country,
        ICurrencyService currency,
        ICustomerActivityService customerActivity,
        ICustomerAttributeParser customerAttributeParser,
        ICustomerAttributeService customerAttribute,
        ICustomerModelFactory customerModelFactory,
        Services.ICustomerRegistrationService customerRegistration,
        ICustomerService customer,
        IDownloadService download,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthentication,
        IGdprService gdpr,
        IGenericAttributeService genericAttribute,
        IGiftCardService giftCard,
        ILocalizationService localization,
        ILogger logger,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscription,
        INotificationService notification,
        IOrderService order,
        IPermissionService permission,
        IPictureService picture,
        IPriceFormatter priceFormatter,
        IProductService product,
        IStateProvinceService stateProvince,
        IShoppingCartService shoppingCart,
        IStoreContext storeContext,
        ITaxService tax,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessage,
        LocalizationSettings localizationSettings,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        TaxSettings taxSettings)
            : base(
                addressSettings,
                captchaSettings,
                customerSettings,
                dateTimeSettings,
                forumSettings,
                gdprSettings,
                htmlEncoder,
                addressAttributeParser,
                addressModelFactory,
                address,
                authentication,
                country,
                currency,
                customerActivity,
                customerAttributeParser,
                customerAttribute,
                customerModelFactory,
                customerRegistration,
                customer,
                download,
                eventPublisher,
                exportManager,
                externalAuthentication,
                gdpr,
                genericAttribute,
                giftCard,
                localization,
                logger,
                multiFactorAuthenticationPluginManager,
                newsLetterSubscription,
                notification,
                order,
                permission,
                picture,
                priceFormatter,
                product,
                stateProvince,
                storeContext,
                tax,
                workContext,
                workflowMessage,
                localizationSettings,
                mediaSettings,
                multiFactorAuthenticationSettings,
                storeInformationSettings,
                taxSettings)
    {
        _captchaSettings = captchaSettings;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _gdprSettings = gdprSettings;
        _address = address;
        _authentication = authentication;
        _country = country;
        _customerActivity = customerActivity;
        _customerAttributeParser = customerAttributeParser;
        _customerAttribute = customerAttribute;
        _customerModelFactory = customerModelFactory;
        _customerRegistration = customerRegistration;
        _customer = customer;
        _eventPublisher = eventPublisher;
        _gdpr = gdpr;
        _genericAttribute = genericAttribute;
        _localization = localization;
        _logger = logger;
        _newsLetterSubscription = newsLetterSubscription;
        _shoppingCart = shoppingCart;
        _stateProvince = stateProvince;
        _storeContext = storeContext;
        _tax = tax;
        _workContext = workContext;
        _workflowMessage = workflowMessage;
        _localizationSettings = localizationSettings;
        _taxSettings = taxSettings;
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _model = model;
        _registrationField = registrationField;
    }
    #endregion Ctor

    #region Utilities

    protected override async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        var attributesXml = string.Empty;
        var attributes = await _customerAttribute.GetAllCustomerAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            var ctrlAttributes = form[controlId];
            if (StringValues.IsNullOrEmpty(ctrlAttributes))
            {
                continue;
            }
            attributesXml = attribute.AttributeControlType switch
            {
                AttributeControlType.DropdownList or AttributeControlType.RadioList
                    when int.TryParse(ctrlAttributes!, CultureInfo.InvariantCulture, out var selectedAttributeId) && selectedAttributeId > 0
                    => _customerAttributeParser.AddCustomerAttribute(attributesXml, attribute, selectedAttributeId.ToString(CultureInfo.InvariantCulture)),
                AttributeControlType.Checkboxes => CheckboxesAttributesXml(attribute, ctrlAttributes),
                AttributeControlType.ReadonlyCheckboxes => await ReadonlyCheckboxesAttributesXml(attribute),
                AttributeControlType.TextBox or AttributeControlType.MultilineTextbox => _customerAttributeParser.AddCustomerAttribute(attributesXml, attribute, ctrlAttributes.ToString().Trim()),
                _ => string.Empty,
            };
        }
        return attributesXml;

        string CheckboxesAttributesXml(CustomerAttribute attribute, StringValues ctrlAttributes)
        {
            foreach (var item in ctrlAttributes
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(item => int.TryParse(item, out var selectedAttributeId) && selectedAttributeId > 0 ? selectedAttributeId.ToString(CultureInfo.InvariantCulture) : null)
                .WhereNotNull())
            {
                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml, attribute, item);
            }
            return attributesXml;
        }

        async Task<string> ReadonlyCheckboxesAttributesXml(CustomerAttribute attribute)
        {
            // load read-only (already server-side selected) values
            var attributeValues = await _customerAttribute.GetCustomerAttributeValuesAsync(attribute.Id);
            foreach (var selectedAttributeId in attributeValues
                .Where(customerAttributeValue => customerAttributeValue.IsPreSelected)
                .Select(customerAttributeValue => customerAttributeValue.Id)
                .ToList())
            {
                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml, attribute, selectedAttributeId.ToString(CultureInfo.InvariantCulture));
            }
            return attributesXml;
        }
    }

    protected virtual async Task LogGdpr(
        Customer customer,
        CustomerInfoModel oldCustomerInfoModel,
        CustomerInfoModel newCustomerInfoModel,
        IFormCollection form)
    {
        try
        {
            // consents
            foreach (var consent in (List<GdprConsent>)(await _gdpr.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList())
            {
                var previousConsentValue = await _gdpr.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    // agree
                    if (previousConsentValue is not true)
                    {
                        await _gdpr.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    }
                }
                else
                {
                    // disagree
                    if (previousConsentValue is not false)
                    {
                        await _gdpr.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                    }
                }
            }

            // newsletter subscriptions
            if (_gdprSettings.LogNewsletterConsent)
            {
                if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                {
                    await _gdpr.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localization.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }
                if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                {
                    await _gdpr.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localization.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }
            }

            // user profile changes
            if (!_gdprSettings.LogUserProfileChanges)
            {
                return;
            }
            if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");
            }
            if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");
            }
            if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");
            }
            if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");
            }
            if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");
            }
            if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");
            }
            if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");
            }
            if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");
            }
            if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");
            }
            if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");
            }
            if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
            {
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");
            }
            if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
            {
                var countryName = (await _country.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.Country")} = {countryName}");
            }
            if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
            {
                var stateProvinceName = (await _stateProvince.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                await _gdpr.InsertLogAsync(
                    customer,
                    0,
                    GdprRequestType.ProfileChanged,
                    $"{await _localization.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
        }
    }

    #endregion Utilities

    /// <summary>
    /// Available even when a store is closed
    /// Available even when navigation is not allowed
    /// </summary>
    /// <param name="checkoutAsGuest"></param>
    [CheckAccessClosedStore(true)]
    [CheckAccessPublicStore(true)]
    [HttpGet]
    public override async Task<IActionResult> Login(bool? checkoutAsGuest)
    {
        var model = await _model.LoginModel(checkoutAsGuest);
        return View(model);
    }

    /// <summary>
    /// Available even when a store is closed
    /// Available even when navigation is not allowed
    /// </summary>
    /// <param name="model"></param>
    /// <param name="returnUrl"></param>
    /// <param name="captchaValid"></param>
    [HttpPost]
    [ValidateCaptcha]
    [CheckAccessClosedStore(true)]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Login([FromForm] Models.Customer.LoginModel model, [FromQuery] string returnUrl, bool captchaValid)
    {
        // validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
        {
            ModelState.AddModelError(string.Empty, await _localization.GetResourceAsync("Common.WrongCaptchaMessage"));
        }
        if (!ModelState.IsValid)
        {
            return View(await _model.LoginModel(model.CheckoutAsGuest));
        }
        try
        {
            if (await _customerRegistration.ValidateCustomerWithIdAsync(model.EmailOrUsername, model.Password) is not { } validationResult)
            {
                return BadRequest($"Cannot authenticate the user with the login {model.EmailOrUsername}");
            }
            return await LoginInner(model, returnUrl, validationResult) switch
            {
                Ok<IActionResult> { Okay: var result } => result,
                _ => View(await _model.LoginModel(model.CheckoutAsGuest))
            };
        }
        catch (Exception exception)
        {
            var errorMessage = $"Cannot sign-in customer with the given email/username {model.EmailOrUsername}.";
            if (exception is ApiException)
            {
                errorMessage += $" ({exception.Message})";
            }
            await _logger.ErrorAsync(errorMessage, exception);
            return View(await _model.LoginModel(model.CheckoutAsGuest));
        }
    }

    [CheckAccessPublicStore(true)]
    [HttpGet]
    public override async Task<IActionResult> Register(string returnUrl)
    {
        // check whether registration is allowed
        if (_customerSettings.UserRegistrationType is UserRegistrationType.Disabled)
        {
            return RedirectToRoute(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.Disabled, returnUrl });
        }
        var model = new RegisterModel();
        model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);
        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    [AutoValidateAntiforgeryToken]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public override async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        // check whether registration is allowed
        if (_customerSettings.UserRegistrationType is UserRegistrationType.Disabled)
        {
            return RedirectToRoute(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.Disabled, returnUrl });
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customer.IsRegisteredAsync(customer))
        {
            await _authentication.SignOutAsync();
            await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));
            await _workContext.SetCurrentCustomerAsync(await _customer.InsertGuestCustomerAsync());
        }
        var store = await _storeContext.GetCurrentStoreAsync();
        customer.RegisteredInStoreId = store.Id;

        // custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        // Parse NexPort registration fields and check for errors
        var registrationFields = await _registrationField.ParseMany(form);
        var registrationFieldWarnings = await _registrationField.GetWarnings(registrationFields);
        foreach (var error in registrationFieldWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        // Parse NexPort registration fields with custom type and check for errors
        var customRegistrationFields = await _registrationField.Parse(form);
        foreach (var error in await _registrationField.GetCustomWarnings(customRegistrationFields))
        {
            ModelState.AddModelError(string.Empty, error);
        }

        // validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
        {
            ModelState.AddModelError(string.Empty, await _localization.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        // GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var messages = (await _gdpr.GetAllConsentsAsync())
                .Where(consent => consent.DisplayDuringRegistration
                    && consent.IsRequired
                    && form[$"consent{consent.Id}"].ToString().Equals("on", StringComparison.OrdinalIgnoreCase))
                .Select(consent => consent.RequiredMessage);
            foreach (var message in messages)
            {
                ModelState.AddModelError(string.Empty, message);
            }
        }
        if (!ModelState.IsValid)
        {
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);
            return View(model);
        }
        var customerUserName = model.Username?.Trim();
        var customerEmail = model.Email?.Trim();
        var isApproved = _customerSettings.UserRegistrationType is UserRegistrationType.Standard;
        var registrationRequest = new CustomerRegistrationRequest(
            customer,
            customerEmail,
            _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
            model.Password,
            _customerSettings.DefaultPasswordFormat,
            store.Id,
            isApproved);
        var registrationResult = await _customerRegistration.RegisterCustomerAsync(registrationRequest);
        if (!registrationResult.Success)
        {
            foreach (var error in registrationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        // properties
        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
        {
            customer.TimeZoneId = model.TimeZoneId;
        }

        // VAT number
        if (_taxSettings.EuVatEnabled)
        {
            customer.VatNumber = model.VatNumber;
            var (vatNumberStatus, _, vatAddress) = await _tax.GetVatNumberStatusAsync(model.VatNumber);
            customer.VatNumberStatusId = (int)vatNumberStatus;
            // send VAT number admin notification
            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
            {
                await _workflowMessage.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
            }
        }

        // form fields
        if (_customerSettings.GenderEnabled)
        {
            customer.Gender = model.Gender;
        }
        if (_customerSettings.FirstNameEnabled)
        {
            customer.FirstName = model.FirstName;
        }
        if (_customerSettings.LastNameEnabled)
        {
            customer.LastName = model.LastName;
        }
        if (_customerSettings.DateOfBirthEnabled)
        {
            customer.DateOfBirth = model.ParseDateOfBirth();
        }
        if (_customerSettings.CompanyEnabled)
        {
            customer.Company = model.Company;
        }
        if (_customerSettings.StreetAddressEnabled)
        {
            customer.StreetAddress = model.StreetAddress;
        }
        if (_customerSettings.StreetAddress2Enabled)
        {
            customer.StreetAddress2 = model.StreetAddress2;
        }
        if (_customerSettings.ZipPostalCodeEnabled)
        {
            customer.ZipPostalCode = model.ZipPostalCode;
        }
        if (_customerSettings.CityEnabled)
        {
            customer.City = model.City;
        }
        if (_customerSettings.CountyEnabled)
        {
            customer.County = model.County;
        }
        if (_customerSettings.CountryEnabled)
        {
            customer.CountryId = model.CountryId;
        }
        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
        {
            customer.StateProvinceId = model.StateProvinceId;
        }
        if (_customerSettings.PhoneEnabled)
        {
            customer.Phone = model.Phone;
        }
        if (_customerSettings.FaxEnabled)
        {
            customer.Fax = model.Fax;
        }

        // save customer attributes
        customer.CustomCustomerAttributesXML = customerAttributesXml;
        await _customer.UpdateCustomerAsync(customer);

        // newsletter
        if (_customerSettings.NewsletterEnabled)
        {
            var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

            // save newsletter value
            var newsletter = await _newsLetterSubscription.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customerEmail, store.Id);
            if (newsletter is not null)
            {
                if (model.Newsletter)
                {
                    newsletter.Active = isNewsletterActive;
                    await _newsLetterSubscription.UpdateNewsLetterSubscriptionAsync(newsletter);

                    // GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                    {
                        await _gdpr.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localization.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    }
                }
            }
            else if (model.Newsletter)
            {
                var newsLetterSubscription = new NewsLetterSubscription
                {
                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    Email = customerEmail,
                    Active = isNewsletterActive,
                    StoreId = store.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                await _newsLetterSubscription.InsertNewsLetterSubscriptionAsync(newsLetterSubscription);
                // GDPR
                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                {
                    await _gdpr.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localization.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }
            }
        }
        if (_customerSettings.AcceptPrivacyPolicyEnabled)
        {
            // privacy policy is required
            // GDPR
            if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
            {
                await _gdpr.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localization.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
            }
        }
        // GDPR
        if (_gdprSettings.GdprEnabled)
        {
            foreach (var consent in (await _gdpr.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration))
            {
                var requestType = form[$"consent{consent.Id}"].ToString().Equals("on", StringComparison.OrdinalIgnoreCase) ? GdprRequestType.ConsentAgree : GdprRequestType.ConsentDisagree;
                await _gdpr.InsertLogAsync(customer, consent.Id, requestType, consent.Message);
            }
        }
        await _registrationField.SaveMany(customer, registrationFields);
        foreach (var customField in customRegistrationFields)
        {
            if (await _registrationField.GetById(customField.Key) is not { } registrationField
                || await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender) is not { } customRender)
            {
                continue;
            }
            await customRender.SaveCustoms(customer, registrationField.Id, customField.Value);
        }
        // insert default address (if possible)
        var defaultAddress = new Address
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Company = customer.Company,
            CountryId = customer.CountryId > 0 ? customer.CountryId : null,
            StateProvinceId = customer.StateProvinceId > 0 ? customer.StateProvinceId : null,
            County = customer.County,
            City = customer.City,
            Address1 = customer.StreetAddress,
            Address2 = customer.StreetAddress2,
            ZipPostalCode = customer.ZipPostalCode,
            PhoneNumber = customer.Phone,
            FaxNumber = customer.Fax,
            CreatedOnUtc = customer.CreatedOnUtc,
        };
        if (await _address.IsAddressValidAsync(defaultAddress))
        {
            // some validation
            if (defaultAddress.CountryId is 0)
            {
                defaultAddress.CountryId = null;
            }
            if (defaultAddress.StateProvinceId is 0)
            {
                defaultAddress.StateProvinceId = null;
            }
            // set default address
            // customer.Addresses.Add(defaultAddress);
            await _address.InsertAddressAsync(defaultAddress);
            await _customer.InsertCustomerAddressAsync(customer, defaultAddress);
            customer.BillingAddressId = defaultAddress.Id;
            customer.ShippingAddressId = defaultAddress.Id;
            await _customer.UpdateCustomerAsync(customer);
        }
        // notifications
        if (_customerSettings.NotifyNewCustomerRegistration)
        {
            await _workflowMessage.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer, _localizationSettings.DefaultAdminLanguageId);
        }
        // raise event
        await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
        var currentLanguage = await _workContext.GetWorkingLanguageAsync();
        switch (_customerSettings.UserRegistrationType)
        {
            case UserRegistrationType.EmailValidation:
            {
                await _genericAttribute.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                await _workflowMessage.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);
                return RedirectToRoute(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
            }
            case UserRegistrationType.AdminApproval:
                return RedirectToRoute(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
            case UserRegistrationType.Standard:
            {
                await _workflowMessage.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);
                await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));
                returnUrl = Url.RouteUrl(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.Standard, returnUrl }) ?? throw new("Missing route.");
                return await _customerRegistration.SignInCustomerAsync(customer, returnUrl, true);
            }
            case UserRegistrationType.Disabled:
                return RedirectToRoute(nameof(RegisterResult), new { resultId = (int)UserRegistrationType.Disabled, returnUrl });
            default:
                return RedirectToRoute(HOMEPAGE);
        }
    }

    private async Task<Result<IActionResult, string>> LoginInner(Models.Customer.LoginModel model, string returnUrl, Services.CustomerLoginResults validationResult)
    {
        return validationResult.LoginResult switch
        {
            Core.Domain.Customers.CustomerLoginResults.Successful => Okay(await HandleSuccessfulLogin(model, returnUrl, validationResult)),
            Core.Domain.Customers.CustomerLoginResults.CustomerNotExist => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist")),
            Core.Domain.Customers.CustomerLoginResults.Deleted => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials.Deleted")),
            Core.Domain.Customers.CustomerLoginResults.NotActive => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials.NotActive")),
            Core.Domain.Customers.CustomerLoginResults.NotRegistered => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered")),
            Core.Domain.Customers.CustomerLoginResults.LockedOut => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials.LockedOut")),
            Core.Domain.Customers.CustomerLoginResults.WrongPassword => Error(await _localization.GetResourceAsync("Account.Login.WrongCredentials")),
            _ => throw new NotImplementedException(),
        };
    }

    private async Task<IActionResult> HandleSuccessfulLogin(Models.Customer.LoginModel model, string returnUrl, Services.CustomerLoginResults validationResult)
    {
        await _shoppingCart.MigrateShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), validationResult.Customer, true);
        await _authentication.SignInAsync(validationResult.Customer, model.RememberMe);
        await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(validationResult.Customer));
        var comment = await _localization.GetResourceAsync("ActivityLog.PublicStore.Login");
        await _customerActivity.InsertActivityAsync(validationResult.Customer, "PublicStore.Login", comment, validationResult.Customer);
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            return RedirectToRoute(HOMEPAGE);
        }
        return Redirect(returnUrl);
    }
}
