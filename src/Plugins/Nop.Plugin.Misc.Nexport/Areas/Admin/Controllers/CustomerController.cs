using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Services;
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
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using System.Globalization;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Services.Events;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
[Route(template: "[area]/[controller]/[action]", Order = int.MinValue)]
public class CustomerController :
    Web.Areas.Admin.Controllers.CustomerController,
    IConsumer<CustomerRegisteredEvent>,
    IConsumer<EntityDeletedEvent<Customer>>
{
    private readonly CustomerSettings _customerSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly ICustomerActivityService _customerActivity;
    private readonly ICustomerAttributeParser _customerAttributeParser;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly Services.ICustomerRegistrationService _customerRegistration;
    private readonly ICustomerService _customer;
    private readonly IGenericAttributeService _genericAttribute;
    private readonly ILocalizationService _localization;
    private readonly INewsLetterSubscriptionService _newsLetterSubscription;
    private readonly INotificationService _notification;
    private readonly IPermissionService _permission;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _store;
    private readonly IRegistrationFieldService _registrationField;
    private readonly IWorkContext _workContext;
    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly ILogger _logger;
    private readonly IUserMappingService _userMapping;
    private readonly IUserService _user;
    private readonly IOrderService _order;
    private static readonly string s_this = ViewUtilities.GetControllerName<CustomerController>();

    #region Constructor

    public CustomerController(
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressAttributeParser addressAttributeParser,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        ICustomerAttributeParser customerAttributeParser,
        ICustomerAttributeService customerAttributeService,
        ICustomerModelFactory customerModelFactory,
        Services.ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IForumService forumService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
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
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        ILogger logger,
        IRegistrationFieldService registrationField,
        IUserMappingService userMapping,
        IUserService user,
        IOrderService order)
        : base(
            customerSettings,
            dateTimeSettings,
            emailAccountSettings,
            forumSettings,
            gdprSettings,
            addressAttributeParser,
            addressService,
            customerActivityService,
            customerAttributeParser,
            customerAttributeService,
            customerModelFactory,
            customerRegistrationService,
            customerService,
            dateTimeHelper,
            emailAccountService,
            eventPublisher,
            exportManager,
            forumService,
            gdprService,
            genericAttributeService,
            localizationService,
            newsLetterSubscriptionService,
            notificationService,
            permissionService,
            queuedEmailService,
            rewardPointService,
            storeContext,
            storeService,
            taxService,
            workContext,
            workflowMessageService,
            taxSettings)
    {
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _customerActivity = customerActivityService;
        _customerAttributeParser = customerAttributeParser;
        _customerModelFactory = customerModelFactory;
        _customerRegistration = customerRegistrationService;
        _customer = customerService;
        _genericAttribute = genericAttributeService;
        _localization = localizationService;
        _newsLetterSubscription = newsLetterSubscriptionService;
        _notification = notificationService;
        _permission = permissionService;
        _storeContext = storeContext;
        _store = storeService;
        _workContext = workContext;
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _logger = logger;
        _registrationField = registrationField;
        _userMapping = userMapping;
        _user = user;
        _order = order;
    }

    #endregion Constructor

    [HttpPost]
    public override async Task<IActionResult> Create(CustomerModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        var (customerAttributesXml, newCustomerRoles) = await CreateValidateModel(model, form);
        if (!ModelState.IsValid)
        {
            return View(await _customerModelFactory.PrepareCustomerModelAsync(model, null, true));
        }
        var customer = await CreateCustomer(model, customerAttributesXml);
        await UpdateNewsletterSubscriptions(model, customer);
        await UpdatePassword(model);
        await UpdateCustomerRoles(newCustomerRoles, customer);
        await ValidateVendor(customer);
        var comment = string.Format(CultureInfo.InvariantCulture, await _localization.GetResourceAsync("ActivityLog.AddNewCustomer"), customer.Id);
        await _customerActivity.InsertActivityAsync("AddNewCustomer", comment, customer);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Customers.Customers.Added"));
        await SaveCustomRegistrationFields(form, customer);
        if (!continueEditing)
        {
            return RedirectToAction(nameof(List));
        }
        return RedirectToAction(nameof(Edit), new { id = customer.Id });
    }

    [HttpPost("Admin/Customer/Edit/{id}")]
    [ActionName("Edit")]
    [FormValueRequired("impersonate")]
    //[ParameterBasedOnFormName("storeId", "storeId")]
    public async Task<IActionResult> Impersonate(int id, [Bind("storeId")] int storeId)
    {
        var store = await _store.GetStoreByIdAsync(storeId);
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.AllowCustomerImpersonation))
        {
            return AccessDeniedView();
        }

        // try to get a customer with the specified id
        var customer = await _customer.GetCustomerByIdAsync(id);
        if (customer is null)
        {
            return RedirectToAction("List");
        }

        if (!customer.Active)
        {
            _notification.WarningNotification(
                await _localization.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("Edit", customer.Id);
        }

        // ensure that a non-admin user cannot impersonate as an administrator
        // otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsAdminAsync(currentCustomer) && await _customer.IsAdminAsync(customer))
        {
            _notification.ErrorNotification(await _localization.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("Edit", customer.Id);
        }

        await _customerActivity.InsertActivityAsync(
            "Impersonation.Started",
            string.Format(CultureInfo.InvariantCulture, await _localization.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id),
            customer);
        await _customerActivity.InsertActivityAsync(
            customer,
            "Impersonation.Started",
            string.Format(CultureInfo.InvariantCulture, await _localization.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), currentCustomer.Email, currentCustomer.Id),
            currentCustomer);

        // ensure login is not required
        customer.RequireReLogin = false;
        await _customer.UpdateCustomerAsync(customer);
        await _genericAttribute.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);
        return Redirect(store.Url);
    }

    [Route("Admin/Customer/Edit/{userId}")]
    [HttpPost]
    [ActionName(nameof(Edit))]
    [FormValueRequired("setnexportuserid")]
    public async Task<IActionResult> MapUser(CustomerModel model, Guid userId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(model.Id) is not { } customer)
        {
            return RedirectToAction(nameof(List), ViewUtilities.GetControllerName<CustomerController>());
        }
        if (await _userMapping.FindByCustomerId(customer.Id) is { } current)
        {
            var updated = new UserMapping
            {
                Id = current.Id,
                UserId = userId,
                NopUserId = current.NopUserId,
            };
            await _userMapping.UpdateUserMapping(updated);
        }
        else if (await _userMapping.InsertUserMapping(new UserMapping { UserId = userId, NopUserId = customer.Id }) is Err<UserMapping> { Error: var error })
        {
            return BadRequest($"The NexPort user GroupGuid {error.UserId} has already been mapped with another user!");
        }
        try
        {
            await _user.SynchronizeContactInfoFromAsync(customer, userId);
            _notification.SuccessNotification($"Successfully updated NexPort user {userId}");
        }
        catch (Exception exception)
        {
            var message = $"Cannot synchronize the contact info for customer {customer.Id} with NexPort user {userId}";
            await _logger.ErrorAsync(message, exception);
            _notification.SuccessNotification("Successfully updated the NexPort user mapping, but failed to synchronize the contact info.");
        }
        return RedirectToAction(nameof(Edit), ViewUtilities.GetControllerName<CustomerController>(), new { id = customer.Id });
    }

    [HttpPost]
    public async Task<IActionResult> GetUserDetails(Guid userId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return ErrorJson(await _localization.GetResourceAsync("Admin.AccessDenied.Description"));
        }
        if (await _user.GetUser(userId) is not { } user)
        {
            var message = $"Cannot get detail information of NexPort user {userId}";
            _notification.ErrorNotification(message);
            return Json(null);
        }
        var response = new
        {
            id = user.UserId,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            internalEmail = user.InternalEmail,
        };
        return Json(response);
    }

    [HttpPost]
    public async Task<IActionResult> SetUser(int customerId, Guid userId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return ErrorJson(await _localization.GetResourceAsync("Admin.AccessDenied.Description"));
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return Json(new { redirectUrl = Url.Action(nameof(List), ViewUtilities.GetControllerName<CustomerController>()) });
        }
        var defaultResponse = new { redirectUrl = Url.Action(nameof(Edit), ViewUtilities.GetControllerName<CustomerController>(), new { id = customer.Id }) };
        try
        {
            if (await _user.GetUser(userId) is { } user)
            {
                return Json(defaultResponse);
            }
            var userMapping = new UserMapping
            {
                UserId = userId,
                NopUserId = customer.Id,
            };
            if (await _userMapping.FindByCustomerId(customer.Id) is not { } currentUserMapping)
            {
                if (await _userMapping.InsertUserMapping(userMapping) is Err<UserMapping> { Error: var error })
                {
                    await logError(error, customer);
                }
            }
            else
            {
                await _userMapping.UpdateUserMapping(userMapping);
            }
            _notification.SuccessNotification("Successfully updated NexPort user mapping.");
            await _user.SynchronizeContactInfoFromAsync(customer, userId);
            return Json(defaultResponse);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"Cannot map and sync customer {customer.Id} with NexPort user GroupGuid {userId}.", exception);
            _notification.ErrorNotification($"Cannot map current customer with NexPort user {userId}.", false);
            return Json(defaultResponse);
        }

        async ValueTask logError(UserMapping userMapping, Customer customer)
        {
            await _logger.ErrorAsync($"Cannot map customer {customer.Id} with NexPort user GroupGuid {userId}");
            var errorMessage = $"Cannot map current customer with NexPort user {userId}";
            var customerEditUrl = Url.Action("Edit", "Customer", new { id = userMapping.NopUserId });
            errorMessage += $". {userMapping}. Click <a href=\"{customerEditUrl}\">here</a> to view the existing customer.";
            _notification.ErrorNotification(errorMessage, false);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CanSetUser(int customerId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return ErrorJson(await _localization.GetResourceAsync("Admin.AccessDenied.Description"));
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return Json(null);
        }
        if (await _userMapping.FindByCustomerId(customerId) is { } userMapping)
        {
            var customerOrders = await _order.SearchOrdersAsync(customerId: customer.Id, pageSize: 10);
            return Json(customerOrders.Count is 0);
        }
        return Json(true);
    }

    [Route("Admin/Customer/Edit/{id}")]
    [HttpPost]
    [ActionName("Edit")]
    [FormValueRequired("syncnexportregistrationfields")]
    public async Task<IActionResult> SyncCustomerRegistrationFieldsWithNexport(CustomerModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(model.Id) is null)
        {
            return RedirectToAction(nameof(List), s_this);
        }
        var queueItem = new SynchronizationQueueItem
        {
            CustomerId = model.Id,
            UtcDateCreated = DateTime.UtcNow,
        };
        await _registrationField.InsertSynchronizationQueueItem(queueItem);
        _notification.SuccessNotification("The customer registration fields has been scheduled to be synchronize with Nexport.");
        return RedirectToAction(nameof(Edit), s_this, new { id = model.Id });
    }

    [HttpPost]
    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        try
        {
            await _user.CreateAndMapNewUserAsync(eventMessage.Customer);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync($"Cannot create and map new NexPort user for customer {eventMessage.Customer.Id}", exception, eventMessage.Customer);
        }
    }

    [HttpPost]
    public async Task HandleEventAsync(EntityDeletedEvent<Customer> eventMessage)
    {
        var deletedCustomer = eventMessage.Entity;
        var userMapping = await _userMapping.FindByCustomerId(deletedCustomer.Id);
        if (userMapping is null)
        {
            return;
        }

        await _userMapping.DeleteUserMapping(userMapping);
    }

    
    private async Task SaveCustomRegistrationFields(IFormCollection form, Customer customer)
    {
        if (!form.TryGetValue("StoreId", out var storeIdValue)
            || !int.TryParse(storeIdValue, out var storeId))
        {
            return;
        }

        var registrationFields = await _registrationField.ParseMany(form, storeId);
        var customRegistrationFields = await _registrationField.Parse(form, storeId);
        await _registrationField.SaveMany(customer, registrationFields);
        foreach (var customField in customRegistrationFields)
        {
            if (await _registrationField.GetById(customField.Key) is { } registrationField
                && await _registrationFieldCustomRenderPluginManager.LoadPluginBySystemNameAsync(registrationField.CustomFieldRender) is { } customRender)
            {
                await customRender.SaveCustoms(customer, registrationField.Id, customField.Value);
            }
        }
    }

    private async Task ValidateVendor(Customer customer)
    {
        // vendors can't access admin area pages
        await EnsureCustomerWithAssociatedVenderIsNotAnAdmin(customer);
        // otherwise, they will have access to ALL products
        await EnsureVendorHasAssociatedVendorAccount(customer);
    }

    private async Task EnsureVendorHasAssociatedVendorAccount(Customer customer)
    {
        if (!await _customer.IsVendorAsync(customer) || customer.VendorId is not default(int))
        {
            return;
        }
        var vendorRole = await _customer.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
        await _customer.RemoveCustomerRoleMappingAsync(customer, vendorRole);
        _notification.ErrorNotification(await _localization.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
    }

    private async Task<Customer> CreateCustomer(CustomerModel model, string customerAttributesXml)
    {
        var customer = model.ToEntity<Customer>();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        customer.CustomerGuid = Guid.NewGuid();
        customer.CreatedOnUtc = DateTime.UtcNow;
        customer.LastActivityDateUtc = DateTime.UtcNow;
        customer.RegisteredInStoreId = currentStore.Id;
        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
        {
            customer.TimeZoneId = model.TimeZoneId;
        }
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
            customer.DateOfBirth = model.DateOfBirth;
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
        customer.CustomCustomerAttributesXML = customerAttributesXml;
        await _customer.InsertCustomerAsync(customer);
        return customer;
    }

    private async Task EnsureCustomerWithAssociatedVenderIsNotAnAdmin(Customer customer)
    {
        if (!await _customer.IsAdminAsync(customer) || customer.VendorId <= 0)
        {
            return;
        }
        customer.VendorId = 0;
        await _customer.UpdateCustomerAsync(customer);
        _notification.ErrorNotification(await _localization.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotbeVendor"));
    }

    private async Task UpdateNewsletterSubscriptions(CustomerModel model, Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Email))
        {
            return;
        }
        var allStores = await _store.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedNewsletterSubscriptionStoreIds?.Contains(store.Id) is true)
            {
                if (await _newsLetterSubscription.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id) is { } newsletterSubscription)
                {
                    // unsubscribed
                    await _newsLetterSubscription.DeleteNewsLetterSubscriptionAsync(newsletterSubscription);
                    continue;
                }
                // subscribed
                var newsLetterSubscription = new NewsLetterSubscription
                {
                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    Email = customer.Email,
                    Active = true,
                    StoreId = store.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                await _newsLetterSubscription.InsertNewsLetterSubscriptionAsync(newsLetterSubscription);
            }
        }
    }

    private async Task UpdatePassword(CustomerModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return;
        }
        var changePassRequest = new ChangePasswordRequest(model.Email, false, _customerSettings.DefaultPasswordFormat, model.Password);
        var changePassResult = await _customerRegistration.ChangePasswordAsync(changePassRequest);
        if (changePassResult.Success)
        {
            return;
        }
        foreach (var changePassError in changePassResult.Errors)
        {
            _notification.ErrorNotification(changePassError);
        }
    }

    private async Task UpdateCustomerRoles(List<CustomerRole> newCustomerRoles, Customer customer)
    {
        foreach (var customerRole in newCustomerRoles)
        {
            // ensure that the current customer cannot add to "Administrators" system role if he's not an admin himself
            if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await _customer.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                continue;
            }
            await _customer.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
        }
        await _customer.UpdateCustomerAsync(customer);
    }

    private async Task<(string, List<CustomerRole>)> CreateValidateModel(CustomerModel model, IFormCollection form)
    {
        if (!string.IsNullOrWhiteSpace(model.Email) && await _customer.GetCustomerByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(string.Empty, "Email is already registered");
        }
        if (!string.IsNullOrWhiteSpace(model.Username)
            && _customerSettings.UsernamesEnabled
            && await _customer.GetCustomerByUsernameAsync(model.Username) is not null)
        {
            ModelState.AddModelError(string.Empty, "Username is already registered");
        }
        // validate customer roles
        var allCustomerRoles = await _customer.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = await allCustomerRoles.Where(customerRole => model.SelectedCustomerRoleIds.Contains(customerRole.Id)).ToListAsync();
        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, new List<CustomerRole>());
        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
            _notification.ErrorNotification(customerRolesError);
        }
        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Count > 0
            && newCustomerRoles.Find(customerRole => customerRole.SystemName == NopCustomerDefaults.RegisteredRoleName) is not null
            && !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localization.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
            _notification.ErrorNotification(await _localization.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }
        // custom customer attributes
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
        if (newCustomerRoles.Count > 0 && newCustomerRoles.Find(customerRole => customerRole.SystemName == NopCustomerDefaults.RegisteredRoleName) is not null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        return (customerAttributesXml, newCustomerRoles);
    }
}
