using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
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
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly NexportService _nexportService;
        private readonly NexportSettings _nexportSettings;
        private readonly ILogger _logger;

        #endregion

        public NexportCustomerRegistrationService(
            CustomerSettings customerSettings,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IRewardPointService rewardPointService,
            IStoreService storeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWorkflowMessageService workflowMessageService,
            RewardPointsSettings rewardPointsSettings,
            ILogger logger,
            NexportService nexportService,
            NexportSettings nexportSettings)
        : base(customerSettings, customerService, encryptionService, eventPublisher,
            genericAttributeService, localizationService, newsLetterSubscriptionService, rewardPointService, storeService, workContext, workflowMessageService, rewardPointsSettings)
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

        public NexportCustomerLoginResults ValidateNexportCustomer(string usernameOrEmail, string password)
        {
            var isValidEmail = usernameOrEmail.IsValidEmail();

            var customer = !isValidEmail ?
                _customerService.GetCustomerByUsername(usernameOrEmail) :
                _customerService.GetCustomerByEmail(usernameOrEmail);

            if (customer == null)
            {
                if (isValidEmail)
                    return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.CustomerNotExist };

                var nexportUserResponse = _nexportService.AuthenticateUser(usernameOrEmail, password);

                if (nexportUserResponse == null)
                    throw new Exception($"Cannot authenticate the user with the login {usernameOrEmail}");

                if (nexportUserResponse.ApiErrorEntity.ErrorCode == ApiErrorEntity.ErrorCodeEnum.AuthenticationError)
                    return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.WrongPassword };

                var nexportUserId = nexportUserResponse.UserId;
                var nexportUserMapping = _nexportService.FindUserMappingByNexportUserId(nexportUserId);

                // Check if Nexport user mapping is existed. If existed, then log the user into the system.
                // Otherwise, create new Nop user and map with the information from Nexport.
                if (nexportUserMapping != null)
                {
                    customer = _customerService.GetCustomerById(nexportUserMapping.NopUserId);
                }
                else
                {
                    var registrationIsApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard ||
                                                 (_customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation);

                    customer = _workContext.CurrentCustomer;

                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        nexportUserResponse.InternalEmail, nexportUserResponse.InternalEmail,
                        CommonHelper.GenerateRandomDigitCode(20),
                        PasswordFormat.Hashed,
                        _storeContext.CurrentStore.Id,
                        registrationIsApproved);

                    var registrationResult = base.RegisterCustomer(registrationRequest);
                    if (!registrationResult.Success)
                        return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotRegistered };

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, nexportUserResponse.FirstName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, nexportUserResponse.LastName);

                    _nexportService.InsertUserMapping(new NexportUserMapping()
                    {
                        NexportUserId = nexportUserId,
                        NopUserId = customer.Id
                    });

                    _logger.Information($"Successfully create new customer for Nexport user {nexportUserId}.", customer: customer);
                }

                //update login details
                customer.FailedLoginAttempts = 0;
                customer.CannotLoginUntilDateUtc = null;
                customer.RequireReLogin = false;
                customer.LastLoginDateUtc = DateTime.UtcNow;

                _customerService.UpdateCustomer(customer);

                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.Successful, NopUserId = customer.Id };
            }

            if (customer.Deleted)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.Deleted };
            if (!customer.Active)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotActive };
            //only registered can login
            if (!customer.IsRegistered())
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.NotRegistered };
            //check whether a customer is locked out
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.LockedOut };

            if (!PasswordsMatch(_customerService.GetCurrentPassword(customer.Id), password))
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

                _customerService.UpdateCustomer(customer);

                return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.WrongPassword };
            }

            //update login details
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);

            return new NexportCustomerLoginResults { LoginResult = CustomerLoginResults.Successful };
        }
    }
}
