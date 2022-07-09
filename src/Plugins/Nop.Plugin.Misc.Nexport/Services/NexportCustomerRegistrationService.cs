using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NexportApi.Model;

using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class NexportCustomerLoginResults
    {
        public CustomerLoginResults LoginResult;

        public int? NopUserId { get; set; }
    }

    public class NexportCustomerRegistrationService : CustomerRegistrationService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly NexportService _nexportService;
        private readonly NexportSettings _nexportSettings;
        private readonly ILogger _logger;

        #endregion

        public NexportCustomerRegistrationService(CustomerSettings customerSettings,
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
            IRewardPointService rewardPointService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            RewardPointsSettings rewardPointsSettings,
            NexportService nexportService,
            NexportSettings nexportSettings,
            ILogger logger)
        : base(customerSettings, actionContextAccessor, authenticationService, customerActivityService, customerService,
            encryptionService, eventPublisher, genericAttributeService, localizationService, multiFactorAuthenticationPluginManager,
            newsLetterSubscriptionService, notificationService, rewardPointService,
            shoppingCartService, storeContext, storeService, urlHelperFactory,
            workContext, workflowMessageService, rewardPointsSettings)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _rewardPointService = rewardPointService;
            _storeService = storeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _workflowMessageService = workflowMessageService;
            _rewardPointsSettings = rewardPointsSettings;
            _nexportService = nexportService;
            _nexportSettings = nexportSettings;
            _logger = logger;
        }

        public async Task<NexportCustomerLoginResults> ValidateNexportCustomerAsync(string usernameOrEmail, string password)
        {
            var isValidEmail = usernameOrEmail.IsValidEmail();

            var customer = !isValidEmail ?
                await _customerService.GetCustomerByUsernameAsync(usernameOrEmail) :
                await _customerService.GetCustomerByEmailAsync(usernameOrEmail);

            if (customer == null)
            {
                var nexportUserResponse = await _nexportService.AuthenticateUserAsync(usernameOrEmail, password);

                if (nexportUserResponse == null)
                    throw new Exception($"Cannot authenticate the user with the login {usernameOrEmail}");

                if (nexportUserResponse.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.AuthenticationError)
                    return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.WrongPassword };

                if (nexportUserResponse.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.ItemNotFound)
                    return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.CustomerNotExist };

                var nexportUserId = nexportUserResponse.UserId;
                var nexportUserMapping = await _nexportService.FindUserMappingByNexportUserId(nexportUserId);

                // Check if Nexport user mapping is existed. If existed, then log the user into the system.
                // Otherwise, create new Nop user and map with the information from Nexport.
                if (nexportUserMapping != null)
                {
                    customer = await _customerService.GetCustomerByIdAsync(nexportUserMapping.NopUserId);
                }
                else
                {
                    var registrationIsApproved =
                        _customerSettings.UserRegistrationType == UserRegistrationType.Standard ||
                        (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation);

                    customer = await _workContext.GetCurrentCustomerAsync();

                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        nexportUserResponse.InternalEmail, nexportUserResponse.InternalEmail,
                        CommonHelper.GenerateRandomDigitCode(20),
                        PasswordFormat.Hashed,
                        (await _storeContext.GetCurrentStoreAsync()).Id,
                        registrationIsApproved);

                    var registrationResult = await base.RegisterCustomerAsync(registrationRequest);
                    if (!registrationResult.Success)
                        return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotRegistered };

                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute,
                        nexportUserResponse.FirstName);
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute,
                        nexportUserResponse.LastName);

                    await _nexportService.InsertUserMapping(new NexportUserMapping()
                    {
                        NexportUserId = nexportUserId,
                        NopUserId = customer.Id
                    });

                    await _logger.InformationAsync($"Successfully create new customer for Nexport user {nexportUserId}.",
                        customer: customer);
                }

                //update login details
                customer.FailedLoginAttempts = 0;
                customer.CannotLoginUntilDateUtc = null;
                customer.RequireReLogin = false;
                customer.LastLoginDateUtc = DateTime.UtcNow;

                await _customerService.UpdateCustomerAsync(customer);

                return new NexportCustomerLoginResults
                {
                    LoginResult = CustomerLoginResults.Successful,
                    NopUserId = customer.Id
                };
            }


            if (customer.Deleted)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.Deleted };
            if (!customer.Active)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotActive };
            //only registered can login
            if (!await _customerService.IsRegisteredAsync(customer))
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotRegistered };
            //check whether a customer is locked out
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.LockedOut };

            if (!PasswordsMatch(await _customerService.GetCurrentPasswordAsync(customer.Id), password))
            {
                //wrong password
                customer.FailedLoginAttempts++;
                if (_customerSettings.FailedPasswordAllowedAttempts > 0 &&
                    customer.FailedLoginAttempts >= _customerSettings.FailedPasswordAllowedAttempts)
                {
                    //lock out
                    customer.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
                    //reset the counter
                    customer.FailedLoginAttempts = 0;
                }

                await _customerService.UpdateCustomerAsync(customer);

                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.WrongPassword };
            }

            //update login details
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomerAsync(customer);

            return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.Successful };
        }
    }
}
