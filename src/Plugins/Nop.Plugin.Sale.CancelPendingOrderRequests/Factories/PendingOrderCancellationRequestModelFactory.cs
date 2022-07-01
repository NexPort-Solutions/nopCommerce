using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;
using Nop.Services.Caching;
using Nop.Web.Areas.Admin.Models.Common;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Factories
{
    public class PendingOrderCancellationRequestModelFactory : IPendingOrderCancellationRequestModelFactory
    {
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStaticCacheManager _cacheManager;
        private readonly CacheKeyService _cacheKeyService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPendingOrderCancellationRequestService _pendingOrderCancellationRequestService;
        private readonly IWorkContext _workContext;

        public PendingOrderCancellationRequestModelFactory(
            ILocalizedModelFactory localizedModelFactory,
            IStaticCacheManager cacheManager,
            CacheKeyService cacheKeyService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPendingOrderCancellationRequestService pendingOrderCancellationRequestService,
            IWorkContext workContext)
        {
            _localizedModelFactory = localizedModelFactory;
            _cacheManager = cacheManager;
            _cacheKeyService = cacheKeyService;
            _customerService = customerService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _pendingOrderCancellationRequestService = pendingOrderCancellationRequestService;
            _workContext = workContext;
        }

        public async Task<PendingOrderCancellationRequestSearchModel>
            PreparePendingOrderCancellationRequestSearchModelAsync(PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //var availableStatusItems = PendingOrderCancellationRequestStatus.Accepted.ToSelectList(false);
            var availableStatusItems = await PendingOrderCancellationRequestStatus.Accepted.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
            {
                searchModel.RequestStatusList.Add(statusItem);
            }

            searchModel.RequestStatusId = -1;
            searchModel.RequestStatusList.Insert(0, new SelectListItem
            {
                Value = "-1",
                Text = await _localizationService.GetResourceAsync("Admin.CancellationRequests.SearchCancellationRequestStatus.All")
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<PendingOrderCancellationRequestListModel>
            PreparePendingOrderCancellationRequestListModelAsync(PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var currentTimeZone = await _dateTimeHelper.GetCurrentTimeZoneAsync();

            var startDateValue = !searchModel.StartDate.HasValue
                ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, currentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue
                ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, currentTimeZone).AddDays(1);
            var cancelRequestStatus = searchModel.RequestStatusId == -1
                ? null
                : (PendingOrderCancellationRequestStatus?)searchModel.RequestStatusId;

            // Get cancellation requests
            var cancellationRequests =
                await _pendingOrderCancellationRequestService.SearchCancellationRequestsAsync(
                    requestStatus: cancelRequestStatus,
                    createdFromUtc: startDateValue, createdToUtc: endDateValue,
                    pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new PendingOrderCancellationRequestListModel().PrepareToGrid(searchModel,
                cancellationRequests, () =>
            {
                return cancellationRequests.Select(cancellationRequest =>
                {
                    var requestModel = Task.Run(async () =>
                    {
                        var requestModel = cancellationRequest.ToModel<PendingOrderCancellationRequestModel>();

                        requestModel.UtcCreatedDate =
                            _dateTimeHelper.ConvertToUserTime(
                                cancellationRequest.UtcCreatedDate,
                                TimeZoneInfo.Utc,
                                await _dateTimeHelper.GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync()));

                        var customer = await _customerService.GetCustomerByIdAsync(cancellationRequest.CustomerId);

                        requestModel.CustomerInfo =
                            customer != null && await _customerService.IsRegisteredAsync(customer)
                                ? customer.Email
                                : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

                        return requestModel;
                    }).Result;

                    return requestModel;
                });
            });

            return model;
        }

        public async Task<SubmitCancellationRequestModel>
            PrepareSubmitCancellationRequestModelAsync(SubmitCancellationRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var workingLanguage = await _workContext.GetWorkingLanguageAsync();
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(PluginDefaults.CancellationRequestReasonsCacheKey, workingLanguage.Id);

            model.OrderId = order.Id;

            model.AvailableCancelReasons = await _cacheManager.GetAsync(cacheKey,
                async () =>
                {
                    var requestReasons = await _pendingOrderCancellationRequestService
                        .GetAllCancellationRequestReasonsAsync();
                    return await requestReasons.SelectAwait(async reason =>
                        new PendingOrderCancellationRequestReasonModel
                        {
                            Id = reason.Id,
                            Name = await _localizationService.GetLocalizedAsync(reason, x => x.Name)
                        }).ToListAsync();
                });

            return model;
        }

        public async Task<PendingOrderCancellationRequestModel> PreparePendingOrderCancellationRequestModelAsync(
            PendingOrderCancellationRequestModel model, PendingOrderCancellationRequest cancellationRequest,
            bool excludeProperties = false)
        {
            if (cancellationRequest == null)
                return model;

            //fill in model values from the entity
            model ??= new PendingOrderCancellationRequestModel
            {
                Id = cancellationRequest.Id,
                CustomerId = cancellationRequest.CustomerId,
            };

            var customer = await _customerService.GetCustomerByIdAsync(cancellationRequest.CustomerId);

            model.UtcCreatedDate = _dateTimeHelper.ConvertToUserTime(
                cancellationRequest.UtcCreatedDate,
                TimeZoneInfo.Utc,
                await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));

            model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
                ? customer.Email
                : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            model.OrderId = cancellationRequest.OrderId;

            if (excludeProperties)
                return model;

            model.ReasonForCancellation = cancellationRequest.ReasonForCancellation;
            model.CustomerComments = cancellationRequest.CustomerComments;
            model.StaffNotes = cancellationRequest.StaffNotes;
            model.RequestStatus = cancellationRequest.RequestStatus;

            return model;
        }

        public async Task<PendingOrderCancellationRequestReasonModel> PreparePendingOrderCancellationRequestReasonModelAsync(
            PendingOrderCancellationRequestReasonModel model,
            PendingOrderCancellationRequestReason cancellationRequestReason, bool excludeProperties = false)
        {
            Func<PendingOrderCancellationRequestReasonLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (cancellationRequestReason != null)
            {
                model ??= cancellationRequestReason.ToModel<PendingOrderCancellationRequestReasonModel>();

                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(
                        cancellationRequestReason,
                        entity => entity.Name,
                        languageId, false, false);
                };
            }

            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        public async Task<PendingOrderCancellationRequestReasonListModel>
            PreparePendingOrderCancellationRequestReasonListModelAsync(PendingOrderCancellationRequestReasonSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var reasons =
                (await _pendingOrderCancellationRequestService.GetAllCancellationRequestReasonsAsync()).ToPagedList(searchModel);

            var model = new PendingOrderCancellationRequestReasonListModel().PrepareToGrid(searchModel, reasons, () =>
            {
                return reasons.Select(reason => reason.ToModel<PendingOrderCancellationRequestReasonModel>());
            });

            return model;
        }
    }
}