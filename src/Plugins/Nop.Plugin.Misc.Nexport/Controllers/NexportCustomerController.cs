using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
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
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
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
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Customer;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using Nop.Plugin.Misc.Nexport.Services;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class NexportCustomerController : BasePublicController
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IDownloadService _downloadService;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomerRenderPluginManager;
        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly NexportService _nexportService;


        #endregion

        #region Ctor

        public NexportCustomerController(
            AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            IDownloadService downloadService,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomerRenderPluginManager,
            INexportPluginModelFactory nexportPluginModelFactory,
            NexportService nexportService)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _downloadService = downloadService;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
            _registrationFieldCustomerRenderPluginManager = registrationFieldCustomerRenderPluginManager;
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _nexportService = nexportService;
        }

        #endregion

        #region Utilities

        protected virtual string ParseCustomCustomerAttributes(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = "";
            var attributes = _customerAttributeService.GetAllCustomerAttributes();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopAttributePrefixDefaults.Customer}{attribute.Id}";
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
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _customerAttributeService.GetCustomerAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
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
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
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

        protected virtual void LogGdpr(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, IFormCollection form)
        {
            try
            {
                //consents
                var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = _gdprService.IsConsentAccepted(consent.Id, _workContext.CurrentCustomer.Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                        {
                            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                        }
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                        {
                            _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                        }
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentDisagree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth().ToString()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = _countryService.GetCountryById(newCustomerInfoModel.CountryId)?.Name;
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName = _stateProvinceService.GetStateProvinceById(newCustomerInfoModel.StateProvinceId)?.Name;
                    _gdprService.InsertLog(customer, 0, GdprRequestType.ProfileChanged, $"{_localizationService.GetResource("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message, exception, customer);
            }
        }

        #endregion

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Login(bool? checkoutAsGuest)
        {
            var model = _nexportPluginModelFactory.PrepareNexportLoginModel(checkoutAsGuest);

            return View("Login", model);
        }

        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        [PublicAntiForgery]
        public virtual IActionResult Login(NexportLoginModel model, string returnUrl, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                var validationResult = (_customerRegistrationService as NexportCustomerRegistrationService)?.ValidateNexportCustomer(model.EmailOrUsername, model.Password);

                if (validationResult != null)
                {
                    var loginResult = validationResult.LoginResult;

                    switch (loginResult)
                    {
                        case CustomerLoginResults.Successful:
                            {
                                Customer customer;

                                if (validationResult.NopUserId != null)
                                {
                                    customer = _customerService.GetCustomerById(validationResult.NopUserId.Value);
                                }
                                else
                                {
                                    customer = _customerSettings.UsernamesEnabled ?
                                        _customerService.GetCustomerByUsername(model.EmailOrUsername) :
                                        _customerService.GetCustomerByEmail(model.EmailOrUsername);
                                }

                                //migrate shopping cart
                                _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                                //sign in new customer
                                _authenticationService.SignIn(customer, model.RememberMe);

                                //raise event
                                _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                                //activity log
                                _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                                    _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                                    return RedirectToRoute("Homepage");

                                return Redirect(returnUrl);
                            }
                        case CustomerLoginResults.CustomerNotExist:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                            break;
                        case CustomerLoginResults.Deleted:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                            break;
                        case CustomerLoginResults.NotActive:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                            break;
                        case CustomerLoginResults.NotRegistered:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                            break;
                        case CustomerLoginResults.LockedOut:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                            break;
                        case CustomerLoginResults.WrongPassword:
                        default:
                            ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                            break;
                    }
                }
            }

            //If we got this far, something failed, redisplay form
            model = _nexportPluginModelFactory.PrepareNexportLoginModel(model.CheckoutAsGuest);

            return View(model);
        }

        [HttpsRequirement(SslRequirement.Yes)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            var model = new RegisterModel();
            model = _customerModelFactory.PrepareRegisterModel(model, false, setDefaultValues: true);

            return View("Register", model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        [PublicAntiForgery]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer.
                _authenticationService.SignOut();

                //raise logged out event
                _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;

            //custom customer attributes
            var customerAttributesXml = ParseCustomCustomerAttributes(form);
            var customerAttributeWarnings = _customerAttributeParser.GetAttributeWarnings(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            // Parse Nexport registration fields and check for errors
            var nexportRegistrationFields = _nexportService.ParseRegistrationFields(form);
            var nexportRegistrationFieldWarnings =
                _nexportService.GetRegistrationFieldWarnings(nexportRegistrationFields);
            foreach (var error in nexportRegistrationFieldWarnings)
            {
                ModelState.AddModelError("", error);
            }

            // Parse Nexport registration fields with custom type and check for errors
            var customRegistrationFields = _nexportService.ParseCustomRegistrationFields(form);
            var customRegistrationFieldsWarnings =
                _nexportService.GetCustomRegistrationFieldWarnings(customRegistrationFields);

            foreach (var error in customRegistrationFieldsWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    model.Email,
                    _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    _storeContext.CurrentStore.Id,
                    isApproved);
                var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.TimeZoneIdAttribute, model.TimeZoneId);
                    }
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberAttribute, model.VatNumber);

                        var vatNumberStatus = _taxService.GetVatNumberStatus(model.VatNumber, out string _, out string vatAddress);
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            _workflowMessageService.SendNewVatSubmittedStoreOwnerNotification(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                    if (_customerSettings.DateOfBirthEnabled)
                    {
                        var dateOfBirth = model.ParseDateOfBirth();
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                    }
                    if (_customerSettings.CompanyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CompanyAttribute, model.Company);
                    if (_customerSettings.StreetAddressEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                    if (_customerSettings.StreetAddress2Enabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                    if (_customerSettings.ZipPostalCodeEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                    if (_customerSettings.CityEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CityAttribute, model.City);
                    if (_customerSettings.CountyEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountyAttribute, model.County);
                    if (_customerSettings.CountryEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CountryIdAttribute, model.CountryId);
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.StateProvinceIdAttribute,
                            model.StateProvinceId);
                    if (_customerSettings.PhoneEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);
                    if (_customerSettings.FaxEnabled)
                        _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FaxAttribute, model.Fax);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        //save newsletter value
                        var newsletter = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.Email, _storeContext.CurrentStore.Id);
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = true;
                                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(newsletter);

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
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
                                _newsLetterSubscriptionService.InsertNewsLetterSubscription(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = model.Email,
                                    Active = true,
                                    StoreId = _storeContext.CurrentStore.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.Newsletter"));
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
                            _gdprService.InsertLog(customer, 0, GdprRequestType.ConsentAgree, _localizationService.GetResource("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = _gdprService.GetAllConsents().Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                _gdprService.InsertLog(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
                        }
                    }

                    //save customer attributes
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                    // Save Nexport registration fields
                    _nexportService.SaveNexportRegistrationFields(customer, nexportRegistrationFields);

                    // Save Nexport custom registration fields
                    foreach (var customField in customRegistrationFields)
                    {
                        var registrationField = _nexportService.GetNexportRegistrationFieldById(customField.Key);
                        if (registrationField != null)
                        {
                            var customRender =
                                _registrationFieldCustomerRenderPluginManager.LoadPluginBySystemName(registrationField
                                    .CustomFieldRender);
                            customRender?.SaveCustomRegistrationFields(registrationField.Id, customField.Value);
                        }
                    }

                    //login customer now
                    if (isApproved)
                        _authenticationService.SignIn(customer, true);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                        LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                        Email = customer.Email,
                        Company = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CompanyAttribute),
                        CountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute) > 0
                            ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.CountryIdAttribute)
                            : null,
                        StateProvinceId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute) > 0
                            ? (int?)_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.StateProvinceIdAttribute)
                            : null,
                        County = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CountyAttribute),
                        City = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.CityAttribute),
                        Address1 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddressAttribute),
                        Address2 = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.StreetAddress2Attribute),
                        ZipPostalCode = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute),
                        PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                        FaxNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FaxAttribute),
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (_addressService.IsAddressValid(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);
                        customer.CustomerAddressMappings.Add(new CustomerAddressMapping { Address = defaultAddress });
                        customer.BillingAddress = defaultAddress;
                        customer.ShippingAddress = defaultAddress;
                        _customerService.UpdateCustomer(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        _workflowMessageService.SendCustomerRegisteredNotificationMessage(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event
                    _eventPublisher.Publish(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            {
                                //email validation message
                                _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                _workflowMessageService.SendCustomerEmailValidationMessage(customer, _workContext.WorkingLanguage.Id);

                                //result
                                return RedirectToRoute("RegisterResult",
                                    new { resultId = (int)UserRegistrationType.EmailValidation });
                            }
                        case UserRegistrationType.AdminApproval:
                            {
                                return RedirectToRoute("RegisterResult",
                                    new { resultId = (int)UserRegistrationType.AdminApproval });
                            }
                        case UserRegistrationType.Standard:
                            {
                                //send customer welcome message
                                _workflowMessageService.SendCustomerWelcomeMessage(customer, _workContext.WorkingLanguage.Id);

                                var redirectUrl = Url.RouteUrl("RegisterResult",
                                    new { resultId = (int)UserRegistrationType.Standard, returnUrl }, _webHelper.CurrentRequestProtocol);
                                return Redirect(redirectUrl);
                            }
                        default:
                            {
                                return RedirectToRoute("Homepage");
                            }
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, redisplay form
            model = _customerModelFactory.PrepareRegisterModel(model, true, customerAttributesXml);

            return View(model);
        }
    }
}
