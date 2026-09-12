using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NexportApi.Client;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Services;
using System.Collections.Generic;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Security;
using System.Text.Encodings.Web;
using Nop.Core.Http.Extensions;

namespace Nop.Plugin.Misc.Nexport.Controllers;

[AutoValidateAntiforgeryToken]
public class NexportCustomerController : BasePublicController
{
    #region Fields

    protected readonly AddressSettings _addressSettings;
    protected readonly CaptchaSettings _captchaSettings;
    protected readonly CustomerSettings _customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings;
    protected readonly ForumSettings _forumSettings;
    protected readonly GdprSettings _gdprSettings;
    protected readonly HtmlEncoder _htmlEncoder;
    protected readonly IAddressModelFactory _addressModelFactory;
    protected readonly IAddressService _addressService;
    protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerModelFactory _customerModelFactory;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly IDownloadService _downloadService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IExportManager _exportManager;
    protected readonly IExternalAuthenticationService _externalAuthenticationService;
    protected readonly IGdprService _gdprService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IGiftCardService _giftCardService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
    protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
    protected readonly INotificationService _notificationService;
    protected readonly IOrderService _orderService;
    protected readonly IPermissionService _permissionService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IProductService _productService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITaxService _taxService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly LocalizationSettings _localizationSettings;
    protected readonly MediaSettings _mediaSettings;
    protected readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
    protected readonly StoreInformationSettings _storeInformationSettings;
    protected readonly TaxSettings _taxSettings;
    private static readonly char[] _separator = [','];

    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly INexportPluginModelFactory _nexportPluginModelFactory;
    private readonly INexportNavigationContextService _nexportNavigationContextService;
    private readonly NexportService _nexportService;

    #endregion

    #region Ctor

    public NexportCustomerController(
        AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        HtmlEncoder htmlEncoder,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDownloadService downloadService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        ILogger logger,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        TaxSettings taxSettings,
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        INexportPluginModelFactory nexportPluginModelFactory,
        INexportNavigationContextService nexportNavigationContextService,
        NexportService nexportService)
    {
        _addressSettings = addressSettings;
        _captchaSettings = captchaSettings;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _forumSettings = forumSettings;
        _gdprSettings = gdprSettings;
        _htmlEncoder = htmlEncoder;
        _addressModelFactory = addressModelFactory;
        _addressService = addressService;
        _addressAttributeParser = addressAttributeParser;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _authenticationService = authenticationService;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerActivityService = customerActivityService;
        _customerModelFactory = customerModelFactory;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _downloadService = downloadService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _externalAuthenticationService = externalAuthenticationService;
        _gdprService = gdprService;
        _genericAttributeService = genericAttributeService;
        _giftCardService = giftCardService;
        _localizationService = localizationService;
        _logger = logger;
        _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
        _newsLetterSubscriptionService = newsLetterSubscriptionService;
        _notificationService = notificationService;
        _orderService = orderService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _stateProvinceService = stateProvinceService;
        _storeContext = storeContext;
        _taxService = taxService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _localizationSettings = localizationSettings;
        _mediaSettings = mediaSettings;
        _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
        _storeInformationSettings = storeInformationSettings;
        _taxSettings = taxSettings;
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _nexportPluginModelFactory = nexportPluginModelFactory;
        _nexportNavigationContextService = nexportNavigationContextService;
        _nexportService = nexportService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = "";
        var attributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                                     .Where(v => v.IsPreSelected)
                                     .Select(v => v.Id)
                                     .ToList())
                        {
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    protected virtual string ResolveSanitizedReturnUrl(string returnUrl)
    {
        var sanitizedReturnUrl = _nexportNavigationContextService.Sanitize(returnUrl, Url);
        if (!string.IsNullOrWhiteSpace(sanitizedReturnUrl))
            return sanitizedReturnUrl;

        return _nexportNavigationContextService.GetSanitizedReturnUrl(Request, Url);
    }

    protected virtual NexportNavigationContextModel CreateNavigationContextModel(string safeReturnUrl)
    {
        return new NexportNavigationContextModel
        {
            LoginRouteName = _nexportNavigationContextService.LoginRouteName,
            RegisterRouteName = _nexportNavigationContextService.RegisterRouteName,
            ReturnUrlParameterName = _nexportNavigationContextService.ReturnUrlParameterName,
            SafeReturnUrl = safeReturnUrl ?? string.Empty
        };
    }

    protected virtual async Task LogGdpr(Customer customer, CustomerInfoModel oldCustomerInfoModel,
        CustomerInfoModel newCustomerInfoModel, IFormCollection form)
    {
        try
        {
            //consents
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
            foreach (var consent in consents)
            {
                var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                {
                    //agree
                    if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    }
                }
                else
                {
                    //disagree
                    if (!previousConsentValue.HasValue || previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                    }
                }
            }

            //newsletter subscriptions
            if (_gdprSettings.LogNewsletterConsent)
            {
                if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
            }

            //user profile changes
            if (!_gdprSettings.LogUserProfileChanges)
                return;

            if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

            if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

            if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

            if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth().ToString()}");

            if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

            if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

            if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

            if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

            if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

            if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

            if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

            if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
            {
                var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
            }

            if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
            {
                var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
        }
    }

    protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IFormCollection form)
    {
        foreach (var consent in consents)
        {
            var controlId = $"consent{consent.Id}";
            var cbConsent = form[controlId];
            if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
            {
                ModelState.AddModelError("", consent.RequiredMessage);
            }
        }
    }

    #endregion

    //available even when a store is closed
    [CheckAccessClosedStore(true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> Login(bool? checkoutAsGuest)
    {
        var safeReturnUrl = ResolveSanitizedReturnUrl(null);
        var model = await _nexportPluginModelFactory.PrepareNexportLoginModelAsync(checkoutAsGuest);
        model.NavigationContext = CreateNavigationContextModel(safeReturnUrl);

        return View("Login", model);
    }

    [HttpPost]
    [ValidateCaptcha]
    //available even when a store is closed
    [CheckAccessClosedStore(true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    [AutoValidateAntiforgeryToken]
    public virtual async Task<IActionResult> Login(NexportLoginModel model, string returnUrl, bool captchaValid)
    {
        returnUrl = ResolveSanitizedReturnUrl(returnUrl);

        model.NavigationContext = CreateNavigationContextModel(returnUrl);

        //validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        if (ModelState.IsValid)
        {
            var customerUserName = model.Username;
            var customerEmail = model.Email;
            var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;

            try
            {
                var nexportCustomerRegistrationService = EngineContext.Current.Resolve<NexportCustomerRegistrationService>();

                var validationResult = await nexportCustomerRegistrationService.ValidateNexportCustomerAsync(
                    model.EmailOrUsername,
                    model.Password,
                    HttpContext.RequestAborted);

                if (validationResult != null)
                {
                    var loginResult = validationResult.LoginResult;

                    switch (loginResult)
                    {
                        case CustomerLoginResults.Successful:
                            {
                                return await _customerRegistrationService.SignInCustomerAsync(
                                    validationResult.Customer,
                                    returnUrl,
                                    model.RememberMe);
                            }
                        case CustomerLoginResults.MultiFactorAuthenticationRequired:
                            {
                                var customerMultiFactorAuthenticationInfo = new CustomerMultiFactorAuthenticationInfo
                                {
                                    UserName = userNameOrEmail,
                                    RememberMe = model.RememberMe,
                                    ReturnUrl = returnUrl
                                };
                                await HttpContext.Session.SetAsync(
                                    NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo,
                                    customerMultiFactorAuthenticationInfo);
                                return RedirectToRoute("MultiFactorVerification");
                            }
                        case CustomerLoginResults.CustomerNotExist:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                            break;
                        case CustomerLoginResults.Deleted:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                            break;
                        case CustomerLoginResults.NotActive:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                            break;
                        case CustomerLoginResults.NotRegistered:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                            break;
                        case CustomerLoginResults.LockedOut:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                            break;
                        case CustomerLoginResults.WrongPassword:
                        default:
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Cannot sign in customer with the provided credentials.";

                if (ex is ApiException exception)
                {
                    errorMsg += $" ({exception.Message})";
                }

                await _logger.ErrorAsync(errorMsg, ex);
                ModelState.AddModelError("", errorMsg);
            }
        }

        //If we got this far, something failed, redisplay form
        model = await _nexportPluginModelFactory.PrepareNexportLoginModelAsync(model.CheckoutAsGuest);
        model.NavigationContext = CreateNavigationContextModel(returnUrl);

        return View(model);
    }

    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> Register(string returnUrl)
    {
        returnUrl = ResolveSanitizedReturnUrl(returnUrl);

        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

        var model = new NexportRegisterModel();
        // nop's PrepareRegisterModelAsync populates the passed instance and returns the same object.
        // We keep the existing NexportRegisterModel reference so the typed NavigationContext payload stays intact.
        await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);
        model.NavigationContext = CreateNavigationContextModel(returnUrl);

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    [AutoValidateAntiforgeryToken]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> Register(NexportRegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        returnUrl = ResolveSanitizedReturnUrl(returnUrl);
        model.NavigationContext = CreateNavigationContextModel(returnUrl);

        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsRegisteredAsync(customer))
        {
            //Already registered customer.
            await _authenticationService.SignOutAsync();

            //raise logged out event
            await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

            //Save a new record
            await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
        }

        var store = await _storeContext.GetCurrentStoreAsync();
        customer.RegisteredInStoreId = store.Id;

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
        foreach (var error in customerAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        // Parse Nexport registration fields and check for errors
        var nexportRegistrationFields = await _nexportService.ParseRegistrationFieldsAsync(form);
        var nexportRegistrationFieldWarnings =
            await _nexportService.GetRegistrationFieldWarnings(nexportRegistrationFields);
        foreach (var error in nexportRegistrationFieldWarnings)
        {
            ModelState.AddModelError("", error);
        }

        // Parse Nexport registration fields with custom type and check for errors
        var customRegistrationFields = await _nexportService.ParseCustomRegistrationFieldsAsync(form);
        var customRegistrationFieldsWarnings =
            await _nexportService.GetCustomRegistrationFieldWarnings(customRegistrationFields);

        foreach (var error in customRegistrationFieldsWarnings)
        {
            ModelState.AddModelError("", error);
        }

        //validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService
                .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

            ValidateRequiredConsents(consents, form);
        }

        if (ModelState.IsValid)
        {
            var customerUserName = model.Username?.Trim();
            var customerEmail = model.Email?.Trim();

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                customerEmail,
                _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                store.Id,
                isApproved);
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    customer.VatNumber = model.VatNumber;

                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                    customer.VatNumberStatusId = (int)vatNumberStatus;
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.ParseDateOfBirth();
                if (_customerSettings.CompanyEnabled)
                    customer.Company = model.Company;
                if (_customerSettings.StreetAddressEnabled)
                    customer.StreetAddress = model.StreetAddress;
                if (_customerSettings.StreetAddress2Enabled)
                    customer.StreetAddress2 = model.StreetAddress2;
                if (_customerSettings.ZipPostalCodeEnabled)
                    customer.ZipPostalCode = model.ZipPostalCode;
                if (_customerSettings.CityEnabled)
                    customer.City = model.City;
                if (_customerSettings.CountyEnabled)
                    customer.County = model.County;
                if (_customerSettings.CountryEnabled)
                    customer.CountryId = model.CountryId;
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    customer.StateProvinceId = model.StateProvinceId;
                if (_customerSettings.PhoneEnabled)
                    customer.Phone = model.Phone;
                if (_customerSettings.FaxEnabled)
                    customer.Fax = model.Fax;

                //save customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;
                await _customerService.UpdateCustomerAsync(customer);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                    //save newsletter value
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customerEmail, store.Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = isNewsletterActive;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                            }
                        }
                        //else
                        //{
                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        //}
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = customerEmail,
                                Active = isNewsletterActive,
                                StoreId = store.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                            }
                        }
                    }
                }

                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                {
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                    {
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                    }
                }

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                    foreach (var consent in consents)
                    {
                        var controlId = $"consent{consent.Id}";
                        var cbConsent = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                        {
                            //agree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                        else
                        {
                            //disagree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                // Save Nexport registration fields
                await _nexportService.SaveNexportRegistrationFields(customer, nexportRegistrationFields);

                // Save Nexport custom registration fields
                foreach (var customField in customRegistrationFields)
                {
                    var registrationField = await _nexportService.GetNexportRegistrationFieldById(customField.Key);
                    if (registrationField != null)
                    {
                        var customRender =
                            await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField
                                .CustomFieldRender);
                        await customRender?.SaveCustomRegistrationFields(customer, registrationField.Id, customField.Value);
                    }
                }

                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Company = customer.Company,
                    CountryId = customer.CountryId > 0
                        ? (int?)customer.CountryId
                        : null,
                    StateProvinceId = customer.StateProvinceId > 0
                        ? (int?)customer.StateProvinceId
                        : null,
                    County = customer.County,
                    City = customer.City,
                    Address1 = customer.StreetAddress,
                    Address2 = customer.StreetAddress2,
                    ZipPostalCode = customer.ZipPostalCode,
                    PhoneNumber = customer.Phone,
                    FaxNumber = customer.Fax,
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (await _addressService.IsAddressValidAsync(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    //customer.Addresses.Add(defaultAddress);

                    await _addressService.InsertAddressAsync(defaultAddress);

                    await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event
                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.EmailValidation:
                        //email validation message
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                        await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                        //result
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });

                    case UserRegistrationType.AdminApproval:
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });

                    case UserRegistrationType.Standard:
                        //send customer welcome message
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                        //raise event
                        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                        returnUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                        return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, true);

                    default:
                        return RedirectToRoute("Homepage");
                }
            }

            //errors
            foreach (var error in registrationResult.Errors)
                ModelState.AddModelError("", error);
        }

        //If we got this far, something failed, redisplay form
        // PrepareRegisterModelAsync enriches the current instance in-place; no reassignment needed.
        await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);
        model.NavigationContext = CreateNavigationContextModel(returnUrl);

        return View(model);
    }
}
