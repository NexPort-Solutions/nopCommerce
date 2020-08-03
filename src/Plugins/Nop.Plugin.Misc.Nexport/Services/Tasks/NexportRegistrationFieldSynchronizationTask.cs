using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Core.Data;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportRegistrationFieldSynchronizationTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly NexportService _nexportService;
        private readonly IRepository<NexportRegistrationFieldSynchronizationQueueItem> _nexportRegistrationFieldSynchronizationQueueRepository;

        private int _batchSize = 100;
        private const int MAX_ATTEMPT_COUNT = 5;

        public NexportRegistrationFieldSynchronizationTask(
            IWidgetPluginManager widgetPluginManager,
            ICustomerService customerService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            ILogger logger,
            IRepository<NexportRegistrationFieldSynchronizationQueueItem> nexportRegistrationFieldSynchronizationQueueRepository,
            NexportService nexportService)
        {
            _widgetPluginManager = widgetPluginManager;
            _customerService = customerService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _logger = logger;
            _nexportRegistrationFieldSynchronizationQueueRepository = nexportRegistrationFieldSynchronizationQueueRepository;
            _nexportService = nexportService;
        }

        public void Execute()
        {
            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                var syncItemIds = _nexportRegistrationFieldSynchronizationQueueRepository.Table
                    .OrderBy(item => item.UtcDateLastAttempt)
                    .ThenBy(item => item.UtcDateCreated)
                    .Select(item => item.Id)
                    .Take(_batchSize).ToList();

                SynchronizeRegistrationFields(syncItemIds);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize registration fields with Nexport", ex);
            }
        }

        public void SynchronizeRegistrationFields(IList<int> queueItemIds)
        {
            try
            {
                foreach (var queueItemId in queueItemIds)
                {
                    try
                    {
                        var syncItem = _nexportRegistrationFieldSynchronizationQueueRepository.GetById(queueItemId);

                        if (syncItem == null)
                            return;

                        _logger.Debug(
                            $"Begin registration field synchronization for customer {syncItem.CustomerId}");

                        var userMapping = _nexportService.FindUserMappingByCustomerId(syncItem.CustomerId);
                        if (userMapping != null)
                        {
                            if (syncItem.Attempt > MAX_ATTEMPT_COUNT)
                            {
                                _nexportRegistrationFieldSynchronizationQueueRepository.Delete(syncItem);
                            }
                            else
                            {
                                try
                                {
                                    var customer = _customerService.GetCustomerById(syncItem.CustomerId);
                                    var currentBillingAddress = customer?.BillingAddress;
                                    if (currentBillingAddress != null)
                                    {
                                        var customerStateProvince =
                                            _stateProvinceService.GetStateProvinceById(currentBillingAddress
                                                .StateProvinceId.GetValueOrDefault(0));

                                        var customerAddressState = customerStateProvince != null ? customerStateProvince.Name : "";

                                        var customerCountry =
                                            _countryService.GetCountryById(currentBillingAddress.CountryId.GetValueOrDefault(0));

                                        var customerAddressCountry = customerCountry != null ? customerCountry.Name : "";

                                        var updatedInfo = new UserContactInfoRequest(apiErrorEntity: new ApiErrorEntity())
                                        {
                                            AddressLine1 = currentBillingAddress.Address1,
                                            AddressLine2 = currentBillingAddress.Address2,
                                            City = currentBillingAddress.City,
                                            State = customerAddressState,
                                            Country = customerAddressCountry,
                                            PostalCode = currentBillingAddress.ZipPostalCode,
                                            Phone = currentBillingAddress.PhoneNumber,
                                            Fax = currentBillingAddress.FaxNumber
                                        };

                                        _nexportService.UpdateNexportUserContactInfo(userMapping.NexportUserId,
                                            updatedInfo);

                                        _logger.Information($"Successfully update contact information in Nexport for customer {userMapping.NopUserId}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error($"Cannot update contact information for customer {userMapping.NopUserId} in Nexport", ex);
                                }

                                var registrationFields =
                                    _nexportService.GetNexportRegistrationFieldAnswers(userMapping.NopUserId);

                                if (registrationFields.Count > 0)
                                {
                                    var nexportSubscriptions =
                                        _nexportService.FindAllSubscriptions(userMapping.NexportUserId);

                                    foreach (var subscription in nexportSubscriptions)
                                    {
                                        var profileFields =
                                            _nexportService.ConvertToSubmissionProfileFields(registrationFields);

                                        try
                                        {
                                            var result = _nexportService.SetCustomProfileFieldValues(subscription.SubscriptionId, profileFields);

                                            if (!string.IsNullOrEmpty(result.Message))
                                            {
                                                _logger.Error($"Error occurred when setting custom profile fields for customer {userMapping.NopUserId}: {result.Message}");
                                            }

                                            _logger.Information($"Successfully synchronize custom profile fields for customer {userMapping.NopUserId} with the subscriber Id {subscription.SubscriptionId}");
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error($"Cannot set custom profile fields for customer {userMapping.NopUserId} in Nexport", ex);

                                            syncItem.Attempt++;

                                            if (syncItem.Attempt <= MAX_ATTEMPT_COUNT)
                                            {
                                                _nexportService.UpdateNexportRegistrationFieldSynchronizationQueueItem(syncItem);
                                            }
                                        }
                                    }

                                    _logger.Information($"Successfully synchronize all custom profile fields for customer {userMapping.NopUserId}");
                                }

                                _nexportRegistrationFieldSynchronizationQueueRepository.Delete(syncItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportRegistrationFieldSynchronizationQueue item with Id {queueItemId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot synchronize registration field with Nexport", ex);
            }
        }
    }
}
