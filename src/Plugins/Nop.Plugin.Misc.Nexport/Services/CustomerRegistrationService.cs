using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using LoginResult = Nop.Core.Domain.Customers.CustomerLoginResults;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ICustomerRegistrationService : Nop.Services.Customers.ICustomerRegistrationService
{
    Task<CustomerLoginResults?> ValidateCustomerWithIdAsync(string usernameOrEmail, string password);
}

public class CustomerRegistrationService : Nop.Services.Customers.CustomerRegistrationService, ICustomerRegistrationService
{
    private readonly CustomerSettings _customerSettings;
    private readonly ICustomerService _customer;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IUserMappingService _userMapping;
    private readonly ILogger _logger;
    private readonly IUserService _user;

    public CustomerRegistrationService(
        CustomerSettings customerSettings,
        IActionContextAccessor actionContextAccessor,
        IAuthenticationService authenticationService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IRewardPointService rewardPointService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IStoreService storeService,
        IUrlHelperFactory urlHelperFactory,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        RewardPointsSettings rewardPointsSettings,
        ILogger logger,
        IUserMappingService userMapping,
        IUserService user)
    : base(
        customerSettings,
        actionContextAccessor,
        authenticationService,
        customerActivityService,
        customerService,
        encryptionService,
        eventPublisher,
        genericAttributeService,
        localizationService,
        multiFactorAuthenticationPluginManager,
        newsLetterSubscriptionService,
        notificationService,
        permissionService,
        rewardPointService,
        shoppingCartService,
        storeContext,
        storeService,
        urlHelperFactory,
        workContext,
        workflowMessageService,
        rewardPointsSettings)
    {
        _customerSettings = customerSettings;
        _customer = customerService;
        _workContext = workContext;
        _storeContext = storeContext;
        _logger = logger;
        _userMapping = userMapping;
        _user = user;
    }

    public async Task<CustomerLoginResults?> ValidateCustomerWithIdAsync(string usernameOrEmail, string password) => await _customer.GetCustomer(usernameOrEmail) switch
    {
        null when await _user.AuthenticateUser(usernameOrEmail, password) is { } userResponse => await LoginOrRegister(userResponse),
        null => null,
        { Deleted: true } customer => new CustomerLoginResults(LoginResult.Deleted, customer),
        { Active: false } customer => new CustomerLoginResults(LoginResult.NotActive, customer),
        // only registered may login
        var customer when !await _customer.IsRegisteredAsync(customer) => new CustomerLoginResults(LoginResult.NotRegistered, customer),
        // check whether a customer is locked out
        { CannotLoginUntilDateUtc: var loginDate } customer when loginDate > DateTime.UtcNow => new CustomerLoginResults(LoginResult.LockedOut, customer),
        // wrong password
        var customer when !PasswordsMatch(await _customer.GetCurrentPasswordAsync(customer.Id), password) => await WrongPassword(customer),
        var customer => await UpdateCustomerLoginInfo(customer),
    };

    private async Task<CustomerLoginResults> WrongPassword(Customer customer)
    {
        customer.FailedLoginAttempts++;
        // lock out and reset attempts counter
        if (_customerSettings.FailedPasswordAllowedAttempts > 0
            && _customerSettings.FailedPasswordAllowedAttempts <= customer.FailedLoginAttempts)
        {
            customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
            customer.FailedLoginAttempts = 0;
        }
        await _customer.UpdateCustomerAsync(customer);
        return new CustomerLoginResults(LoginResult.WrongPassword, customer);
    }

    private async Task<CustomerLoginResults> LoginOrRegister(GetUserResponse userResponse)
    {
        if (userResponse.ApiErrorEntity.ErrorCode is ApiErrorEntity.ErrorCodeEnum.ItemNotFound)
        {
            return new CustomerLoginResults(LoginResult.CustomerNotExist, null);
        }
        // If user mapping exists, log the user into the system.
        // Else, create new Nop user and map with the information from NexPort.
        if (await _userMapping.FindUserMappingByUserId(userResponse.UserId) is { } userMapping)
        {
            return await UpdateCustomerLoginInfo(await _customer.GetCustomerByIdAsync(userMapping.NopUserId));
        }
        return await RegisterCustomer(userResponse);
    }

    private async Task<CustomerLoginResults> RegisterCustomer(GetUserResponse userResponse)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var registrationRequest = new CustomerRegistrationRequest(
            customer,
            userResponse.InternalEmail,
            userResponse.InternalEmail,
            CommonHelper.GenerateRandomDigitCode(20),
            PasswordFormat.Hashed,
            (await _storeContext.GetCurrentStoreAsync()).Id,
            _customerSettings.UserRegistrationType is UserRegistrationType.Standard or UserRegistrationType.EmailValidation);
        if (await RegisterCustomerAsync(registrationRequest) is not { Success: true })
        {
            return new CustomerLoginResults(LoginResult.NotRegistered, customer);
        }
        customer.FirstName = userResponse.FirstName;
        customer.LastName = userResponse.LastName;
        await _customer.UpdateCustomerAsync(customer);
        var userMapping = new UserMapping
        {
            UserId = userResponse.UserId,
            NopUserId = customer.Id,
        };
        await _userMapping.InsertUserMapping(userMapping);
        await _logger.InformationAsync($"Successfully create new customer for NexPort user {userResponse.UserId}.", customer: customer);
        return await UpdateCustomerLoginInfo(customer);
    }

    private async Task<CustomerLoginResults> UpdateCustomerLoginInfo(Customer customer)
    {
        customer.FailedLoginAttempts = 0;
        customer.CannotLoginUntilDateUtc = null;
        customer.RequireReLogin = false;
        customer.LastLoginDateUtc = DateTime.UtcNow;
        await _customer.UpdateCustomerAsync(customer);
        return new CustomerLoginResults(LoginResult.Successful, customer);
    }
}

public record CustomerLoginResults(LoginResult LoginResult, Customer? Customer);
