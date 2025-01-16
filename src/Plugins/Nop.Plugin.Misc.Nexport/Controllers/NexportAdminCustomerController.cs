using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public class NexportAdminCustomerController(
    CustomerSettings customerSettings,
    DateTimeSettings dateTimeSettings,
    EmailAccountSettings emailAccountSettings,
    ForumSettings forumSettings,
    GdprSettings gdprSettings,
    IAddressService addressService,
    IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
    IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
    IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
    ICustomerActivityService customerActivityService,
    ICustomerModelFactory customerModelFactory,
    ICustomerRegistrationService customerRegistrationService,
    ICustomerService customerService,
    IDateTimeHelper dateTimeHelper,
    IEmailAccountService emailAccountService,
    IEventPublisher eventPublisher,
    IExportManager exportManager,
    IForumService forumService,
    IGdprService gdprService,
    IGenericAttributeService genericAttributeService,
    IImportManager importManager,
    ILocalizationService localizationService,
    INewsLetterSubscriptionService newsLetterSubscriptionService,
    INotificationService notificationService,
    IPermissionService permissionService,
    IQueuedEmailService queuedEmailService,
    IRewardPointService rewardPointService,
    IStoreContext storeContext,
    IStoreService storeService,
    ITaxService taxService,
    IWorkContext workContext,
    IWorkflowMessageService workflowMessageService,
    TaxSettings taxSettings,
    NexportService nexportService,
    IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
    ILogger logger)
    : CustomerController(customerSettings, dateTimeSettings, emailAccountSettings, forumSettings, gdprSettings,
        addressService, addressAttributeParser, customerAttributeParser, customerAttributeService,
        customerActivityService,
        customerModelFactory, customerRegistrationService, customerService,
        dateTimeHelper, emailAccountService, eventPublisher, exportManager, forumService, gdprService,
        genericAttributeService,
        importManager, localizationService, newsLetterSubscriptionService, notificationService, permissionService,
        queuedEmailService,
        rewardPointService, storeContext, storeService, taxService, workContext, workflowMessageService, taxSettings)
{
    #region Fields

    protected readonly CustomerSettings _customerSettings = customerSettings;
    protected readonly DateTimeSettings _dateTimeSettings = dateTimeSettings;
    protected readonly EmailAccountSettings _emailAccountSettings = emailAccountSettings;
    protected readonly ForumSettings _forumSettings = forumSettings;
    protected readonly GdprSettings _gdprSettings = gdprSettings;
    protected readonly IAddressService _addressService = addressService;
    protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser = addressAttributeParser;
    protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser = customerAttributeParser;
    protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService = customerAttributeService;
    protected readonly ICustomerActivityService _customerActivityService = customerActivityService;
    protected readonly ICustomerModelFactory _customerModelFactory = customerModelFactory;
    protected readonly ICustomerRegistrationService _customerRegistrationService = customerRegistrationService;
    protected readonly ICustomerService _customerService = customerService;
    protected readonly IDateTimeHelper _dateTimeHelper = dateTimeHelper;
    protected readonly IEmailAccountService _emailAccountService = emailAccountService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IExportManager _exportManager = exportManager;
    protected readonly IForumService _forumService = forumService;
    protected readonly IGdprService _gdprService = gdprService;
    protected readonly IGenericAttributeService _genericAttributeService = genericAttributeService;
    protected readonly IImportManager _importManager;
    protected readonly ILocalizationService _localizationService = localizationService;
    protected readonly INewsLetterSubscriptionService _newsLetterSubscriptionService = newsLetterSubscriptionService;
    protected readonly INotificationService _notificationService = notificationService;
    protected readonly IPermissionService _permissionService = permissionService;
    protected readonly IQueuedEmailService _queuedEmailService = queuedEmailService;
    protected readonly IRewardPointService _rewardPointService = rewardPointService;
    protected readonly IStoreContext _storeContext = storeContext;
    protected readonly IStoreService _storeService = storeService;
    protected readonly ITaxService _taxService = taxService;
    protected readonly IWorkContext _workContext = workContext;
    protected readonly IWorkflowMessageService _workflowMessageService = workflowMessageService;
    protected readonly TaxSettings _taxSettings = taxSettings;

    #endregion

    #region Constructor

    #endregion

    public override async Task<IActionResult> Create(CustomerModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AccessDeniedView();

        if (!string.IsNullOrWhiteSpace(model.Email) && await _customerService.GetCustomerByEmailAsync(model.Email) != null)
            ModelState.AddModelError(string.Empty, "Email is already registered");

        if (!string.IsNullOrWhiteSpace(model.Username) && _customerSettings.UsernamesEnabled &&
            await _customerService.GetCustomerByUsernameAsync(model.Username) != null)
        {
            ModelState.AddModelError(string.Empty, "Username is already registered");
        }

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = new List<CustomerRole>();
        foreach (var customerRole in allCustomerRoles)
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                newCustomerRoles.Add(customerRole);
        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, new List<CustomerRole>());
        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
            _notificationService.ErrorNotification(customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        //custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Any() && newCustomerRoles.FirstOrDefault(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            var customer = model.ToEntity<Customer>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            customer.CustomerGuid = Guid.NewGuid();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            customer.RegisteredInStoreId = currentStore.Id;

            //form fields
            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                customer.TimeZoneId = model.TimeZoneId;
            if (_customerSettings.GenderEnabled)
                customer.Gender = model.Gender;
            if (_customerSettings.FirstNameEnabled)
                customer.FirstName = model.FirstName;
            if (_customerSettings.LastNameEnabled)
                customer.LastName = model.LastName;
            if (_customerSettings.DateOfBirthEnabled)
                customer.DateOfBirth = model.DateOfBirth;
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
            customer.CustomCustomerAttributesXML = customerAttributesXml;

            await _customerService.InsertCustomerAsync(customer);

            //newsletter subscriptions
            if (!string.IsNullOrEmpty(customer.Email))
            {
                var allStores = await _storeService.GetAllStoresAsync();
                foreach (var store in allStores)
                {
                    var newsletterSubscription = await _newsLetterSubscriptionService
                        .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                        model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                    {
                        //subscribed
                        if (newsletterSubscription == null)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                            {
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                Email = customer.Email,
                                Active = true,
                                StoreId = store.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        //not subscribed
                        if (newsletterSubscription != null)
                        {
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletterSubscription);
                        }
                    }
                }
            }

            //password
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
                var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (!changePassResult.Success)
                {
                    foreach (var changePassError in changePassResult.Errors)
                        _notificationService.ErrorNotification(changePassError);
                }
            }

            //customer roles
            foreach (var customerRole in newCustomerRoles)
            {
                //ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
                if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                    continue;

                await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
            }

            await _customerService.UpdateCustomerAsync(customer);

            //ensure that a customer with a vendor associated is not in "Administrators" role
            //otherwise, he won't have access to other functionality in admin area
            if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
            {
                customer.VendorId = 0;
                await _customerService.UpdateCustomerAsync(customer);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotbeVendor"));
            }

            //ensure that a customer in the Vendors role has a vendor account associated.
            //otherwise, he will have access to ALL products
            if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
            {
                var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomer"), customer.Id), customer);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Added"));

            try
            {
                form.TryGetValue("StoreId", out var storeIdValue);
                var storeId = string.IsNullOrWhiteSpace(storeIdValue) ? null : ((int?)int.Parse(storeIdValue));
                if (storeId != null)
                {
                    // Parse Nexport registration fields and check for errors
                    var nexportRegistrationFields = await nexportService.ParseRegistrationFieldsAsync(form, storeId);

                    // Parse Nexport registration fields with custom type and check for errors
                    var customRegistrationFields = await nexportService.ParseCustomRegistrationFieldsAsync(form, storeId);

                    // Save Nexport registration fields
                    await nexportService.SaveNexportRegistrationFields(customer, nexportRegistrationFields);

                    // Save Nexport custom registration fields
                    foreach (var customField in customRegistrationFields)
                    {
                        var registrationField = await nexportService.GetNexportRegistrationFieldById(customField.Key);
                        if (registrationField != null)
                        {
                            var customRender = await registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender);
                            await customRender?.SaveCustomRegistrationFields(customer, registrationField.Id, customField.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"Unable to create registration field answers for customer {customer.Id}!", ex);
            }

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost("Admin/Customer/Edit/{id}"), ActionName("Edit")]
    [FormValueRequired("impersonate")]
    //[ParameterBasedOnFormName("storeId", "storeId")]
    public async Task<IActionResult> Impersonate(int id, [Bind("storeId")] int storeId)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId);
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AllowCustomerImpersonation))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        if (!customer.Active)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("Edit", customer.Id);
        }

        //ensure that a non-admin user cannot impersonate as an administrator
        //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsAdminAsync(currentCustomer) && await _customerService.IsAdminAsync(customer))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("Edit", customer.Id);
        }

        //activity log
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
        await _customerActivityService.InsertActivityAsync(customer, "Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), currentCustomer.Email, currentCustomer.Id), currentCustomer);

        //ensure login is not required
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

        return Redirect(store.Url);
    }
}